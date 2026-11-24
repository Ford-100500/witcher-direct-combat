# Подготовка к публикации

Репозиторий готов к публикации, но **никуда не отправлен**. Ниже — состояние и оставшиеся шаги.

## Текущее состояние

- Ветка: `main`.
- Remote: `upstream` → [VeggieVampire/witcher-direct-combat](https://github.com/VeggieVampire/witcher-direct-combat). `origin` не настроен — его нужно добавить после создания форка.
- История: оригинальный коммит Nick Farrow + коммиты форка.
- Рабочее дерево чистое; `location.txt`, `nextweapon.bak` и логи в `.gitignore`.
- Версия в `CHANGELOG.md`: 0.2.0.

## Что подготовлено

- `README.md` — описание мода, изменения форка, требования, управление, ограничения.
- `docs/CONTROLS.md` — полная карта управления.
- `docs/INSTALL.md` — установка, проверка, обновление, удаление, частые проблемы.
- `CHANGELOG.md` — изменения относительно оригинала.
- `LICENSE` — MIT с сохранённым копирайтом Nick Farrow.
- `.gitignore`, `.gitattributes`.
- `Install.cmd` / `Install.ps1`, `Uninstall.cmd` / `Uninstall.ps1`.
- `Payload/` — исходник, готовая сборка контроллера, `Play.cmd`, Lua-хук.
- Проверено: PowerShell-скрипты парсятся, сборка в `Payload` совпадает с установленной, личных путей в репозитории нет.

## Проверка перед публикацией

```bash
git status
git log --oneline -n 10
git remote -v
```

Убедись, что:

1. `git status` чистый.
2. В `README.md` и `docs/` нет обещаний несуществующих функций.
3. `Payload/DirectCombatController.exe` — свежая сборка из `Payload/DirectCombatController.cs`.
4. В репозитории нет `location.txt`, `nextweapon.bak`, логов и файлов игры.
5. Копирайт Nick Farrow на месте в `LICENSE` и шапке `Payload/DirectCombatController.cs`.

## Автор коммитов

Коммиты этой сессии подписаны `OpenCode <opencode@local>`. Если нужен свой автор:

```bash
cd "путь/к/Witcher Direct Combat"
git config user.name "Ваше Имя"
git config user.email "you@example.com"
```

Переписать автора у последнего коммита:

```bash
git commit --amend --reset-author --no-edit
```

У всех коммитов сразу:

```bash
git rebase -i --root
# в редакторе для каждого коммита:
# exec git commit --amend --reset-author --no-edit
```

## Создание форка и push

1. Открой [VeggieVampire/witcher-direct-combat](https://github.com/VeggieVampire/witcher-direct-combat).
2. Нажми **Fork** и создай форк в своём аккаунте.
3. Добавь свой remote (origin сейчас отсутствует):

```bash
git remote add origin https://github.com/<ваш-аккаунт>/witcher-direct-combat.git
git push -u origin main
```

`upstream` уже указывает на оригинал — он останется для синхронизации.

Если форк не нужен, создай пустой репозиторий и используй его как `origin`. Ссылку на оригинального автора в README сохрани.

## После push

- Заполни описание и темы: `witcher`, `mod`, `gamepad`, `xinput`, `the-witcher-1`.
- Проверь, что GitHub корректно показывает README и ссылки на `docs/`.
- При желании сделай релиз `v0.2.0`.

Состав ZIP для релиза:

```text
Install.cmd
Install.ps1
Uninstall.cmd
Uninstall.ps1
LICENSE
README.md
CHANGELOG.md
Payload/
docs/
```

Не включай `location.txt`, `nextweapon.bak`, логи и файлы игры.

## Чего не делать

- Не пушить `location.txt` и `nextweapon.bak`.
- Не менять копирайт Nick Farrow.
- Не обещать радиальное меню и вкладки инвентаря — они ещё не сделаны.
