# ShadowBound — Мрачный 2D Souls-like экшен от одного разработчика

[![itch.io](https://img.shields.io/badge/itch.io-Скачать_игру-orange?logo=itch.io&style=for-the-badge)](https://dmitriy-spirikhin.itch.io/shadowbound)
[![Unity 2022 LTS](https://img.shields.io/badge/Unity-6000.2-222222?logo=unity&style=for-the-badge)](https://unity.com)
[![C#](https://img.shields.io/badge/C%23-100%25-239120?logo=csharp&style=for-the-badge)]()

> «Тени не убивают — они лишь ждут, пока ты устанешь бороться…»

**ShadowBound** — мрачный 2D экшен-платформер с элементами RPG и roguelike, созданный **полностью в одиночку**: код, пиксель-арт, звуки, UI и сюжет — всё разработано мной без использования ИИ для генерации ассетов. Игра вдохновлена *Dead Cells*, *Hollow Knight* и *Salt and Sanctuary*, но предлагает собственный взгляд на боевую глубину и атмосферу мрачного фэнтези.

---

## 🔥 Ключевые особенности

### ⚔️ Тактическая боевая система уровня Souls,Sekiro-like
- **Frame-perfect парирование** с замедлением времени, тряской камеры и тактильной отдачей
- **Комбо-механика через концентрацию**: каждый парированный удар снижают концентрацию, когда закончиться можно добавить врага уникальной кровавой анимацией.
- **Четыре типа ударов** с уникальной геометрией:
  - `Pierce` — пронзающий (1 направление)
  - `Slash` — рубящий (3 направления)
  - `SemiCircleSlash` — полукруговой (5 направлений)
  - `Circle` — круговой (8 направлений)
- **Удары в спину и экзекуции**
- **Магия и амулеты**: огненные шары, вампиризм, контр-урон

### ❤️ Двойная система здоровья с кровотечением
- После урона временное здоровье уменьшается сначала, можно восстановить своевременным ударом по врагу.
- **Механика кровотечения**: после урона настоящее здоровье медленно уменьшается до уровня temp health - урон
- **Реактивное управление состоянием** через `IReadOnlyReactiveProperty<HealthState>`
- **Визуальная обратная связь**: тряска камеры при уронах >20% от максимума

### 🩸 Оптимизированные визуальные эффекты
- **Пул крови и следов**: предварительно инстанцированные объекты без рантайм-аллокаций
- **Адаптивная интенсивность**: количество частиц пропорционально урону (`Emit(урон / 2)`)
- **Случайные вариации**: 5+ спрайтов следов и цветовых схем для органичности

### 👁️ Адаптивный ИИ
- **Многоуровневое восприятие**: враги различают зоны (дальняя патрулирования, ближняя атаки, координация)
- **Стейт-машина с реактивными переходами**: `Patrol → Chase → Attack → Stunned`

### 📱 Мобильная оптимизация
- **Стабильные 60 FPS** даже на mid-range Android (Snapdragon 665+)
- **Отзывчивое управление**: stick + кнопки экрана
- **Адаптивный UI**: масштабируется под любой экран без потери читаемости

---

## 🛠 Архитектурные решения высокого уровня

### 1. Единая система боя через абстракции (`CombatSystem.cs`)

```csharp
// Геометрия ударов как данные — не хардкод
private Dictionary<SlashType, Vector2[]> _slashVectors = new Dictionary<SlashType, Vector2[]>()
    {
       { SlashType.Pierce, new Vector2[] { new Vector2(1, 0) } },
       { SlashType.Slash, new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)} },
       { SlashType.SemiCircleSlash, new Vector2[] { new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)} },
       { SlashType.Circle, new Vector2[] {new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1)}},
    };

    void Awake()
    {
        _concentrationSystem = GetComponent<ConcentrationSystem>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Hit(SlashType slashType)
    {
        bool isMiss = true;
        _enemiesSet.Clear();
        if (_slashVectors.TryGetValue(slashType, out Vector2[] vectors))
        {
            if (vectors.Length > 0)
            {
                for (int i = 0; i < vectors.Length; i++)
                {
                    RaycastHit2D hit = Raycast(vectors[i]);
                    if (hit.collider != null)
                    {
                        IDestructable destructable = hit.transform.GetComponent<IDestructable>();
                        if (destructable != null && !_enemiesSet.Contains(destructable))
                        {
                            _enemiesSet.Add(destructable);

                            bool isCrit = successHitCounter > 0 && Random.Range(0, 10) < (2 + successHitCounter);

                            DamageData damData = new DamageData(GetDamage(isCrit), (int)-transform.localScale.x, isCrit, weaponType, 2f, 0, transform.localScale.x == hit.transform.localScale.x);

                            if (hit.transform.TryGetComponent<IHealth>(out IHealth health))
                            {
                               // first bool if hit registered , second if hit was parried
                               (bool, bool) hitResult = health.TakeDamage(damData);
                               if (hitResult.Item2) // parried
                               {
                                  _animationService.SetState(_animator, AnimStates.Staggered, true);
                                  _concentrationSystem.ReduceConcentration(reduceConcentrationAmount);
                                  successHitCounter = 0;
                                  return;
                               }
                               else if (hitResult.Item1 && successHitCounter < 5)
                                {
                                    _concentrationSystem.RestoreConcentration(baseRestoreConcentrationAmount * successHitCounter);
                                    successHitCounter++;
                                }
                               else successHitCounter = 0; 
                                                 
                            }
                            else destructable.Destruct(damData);
                        }
                        isMiss = false;
                    }
                }
            }
        }
        else Debug.LogWarning("No value in slashVectors");

        if(isMiss) 
        {
            Debug.Log("AudioMaster is called");
            _audioService.PlayHitSound(_audioSource, WeaponType.Miss);
        }
        
        // Start/reset counter reset timer
        if (_resetCounterRoutine != null) StopCoroutine(_resetCounterRoutine);
        _resetCounterRoutine = StartCoroutine(ResetSuccsessHitCounter());
    }
