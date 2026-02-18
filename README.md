# ShadowBound — Мрачный 2D Souls-like экшен от одного разработчика

[![itch.io](https://img.shields.io/badge/itch.io-Скачать_игру-orange?logo=itch.io&style=for-the-badge)](https://dmitriy-spirikhin.itch.io/shadowbound)
[![Unity 2022 LTS](https://img.shields.io/badge/Unity-6000.2-222222?logo=unity&style=for-the-badge)](https://unity.com)
[![C#](https://img.shields.io/badge/C%23-100%25-239120?logo=csharp&style=for-the-badge)]()

**ShadowBound** — это мобильная Action-RPG с элементами Souls-like. Проект представляет собой результат глубокого рефакторинга оригинальной игры, целью которого был переход от монолитной архитектуры к гибкой, событийно-ориентированной системе

---

### 🏗 Архитектурные решения (Core Focus)
Главная особенность этого демо — демонстрация того, как "инди-код" переводится на профессиональные рельсы:
1. Finite State Machine (FSM) для ИИ и Игрока
Вместо громоздких условий if/else, боевая логика вынесена в отдельные стейты:
Decoupling: Логика каждого состояния (Attack, Parry, Stagger, Patrol) инкапсулирована.
Scalability: Добавление новой фазы боссу или особого типа атаки игрока не затрагивает основной контроллер.
2. ScriptableObject-Driven Architecture
Вся мета-информация и баланс вынесены из кода в ассеты:
Items & Stats: Характеристики амулетов, оружия и врагов настраиваются через ScriptableObjects без перекомпиляции.
Data/Logic Separation: Код отвечает только за выполнение правил, данные хранятся в памяти Unity.

📁 Структура кода (Code Map)
Assets/GameLogic/Core/ — Базовые классы, интерфейсы, архитектурные паттерны.
Assets/GameLogic/Game/ — Логика персонажа и ИИ (Controllers, States).
Assets/Scripts/Items/ — Система инвентаря и описание предметов через SO.
Assets/Scripts/Systems/ — Глобальные менеджеры и логика игрового цикла.

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
