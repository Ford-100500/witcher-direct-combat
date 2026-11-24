# Подготовка к публикации

Репозиторий готов к публикации, но **никуда не отправлен**. Ниже — состояние и оставшиеся шаги.

## Текущее состояние

- Ветка: `main`.
- Remote: `upstream` → [VeggieVampire/witcher-direct-combat](https://github.com/VeggieVampire/witcher-direct-combat). `origin` не настроен — его нужно добавить после создания форка.
- История: оригинальный коммит Nick Farrow + коммиты форка.
- Рабочее дерево чистое; `location.txt`, `nextweapon.bak` и логи в `.gitignore`.
- Версия в `CHANGELOG.md`: 0.3.0.

## Что подготовлено

- `README.md` — описание мода, изменения форка, требования, управление, ограничения.
- `docs/CONTROLS.md` — полная карта управления.
- `docs/INSTALL.md` — установка, проверка, обновление, удаление, частые проблемы.
- `docs/KNOWN-ISSUES.md` — честный список ограничений и хрупких мест.
- `CHANGELOG.md` — изменения относительно оригинала.
- `LICENSE` — MIT с сохранённым копирайтом Nick Farrow.
- `.gitignore`, `.gitattributes`.
- `Install.cmd` / `Install.ps1`, `Uninstall.cmd` / `Uninstall.ps1`.
- `Payload/` — исходник, готовая сборка контроллера, `Play.cmd`, Lua-хук.
- `installer/` — исходник GUI-установщика, `Build.ps1`, тексты для архива.
- Проверено: PowerShell-скрипты парсятся, установщик ставит и удаляет мод на тестовой папке, сборка в `Payload` совпадает с установленной, личных путей в репозитории нет.

**Готовый Setup.exe публикуется в релизе** `v0.3.0` вместе с `WitcherDirectCombat-Setup-0.3.0.zip`. В git-репозиторий EXE не добавляется — только в release assets.

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

Коммиты форка уже подписаны `Ford-100500 <317495324+Ford-100500@users.noreply.github.com>`; оригинальный коммит Nick Farrow не изменён. Если нужен другой автор — настрой `git config user.name` / `user.email` и используй `git commit --amend --reset-author` или `git rebase --exec`.

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
- Релиз можно сделать. Для `v0.3.0` приложи `WitcherDirectCombatSetup.exe` и `WitcherDirectCombat-Setup-0.3.0.zip`; GitHub дополнительно отдаст исходники автоматически.
- `dist/` по-прежнему в `.gitignore`: EXE и ZIP не коммитятся, а загружаются в release assets.

## Чего не делать

- Не пушить `location.txt` и `nextweapon.bak`.
- Не менять копирайт Nick Farrow.
- Не обещать радиальное меню и вкладки инвентаря — они ещё не сделаны.
