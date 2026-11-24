# Подготовка к публикации

Репозиторий готов, но **никуда не отправлен**. Ниже — что осталось сделать вручную.

## Что уже подготовлено

- README с описанием мода, изменениями форка, требованиями и управлением.
- `docs/CONTROLS.md` — карта управления.
- `docs/INSTALL.md` — установка, проверка, обновление, удаление, частые проблемы.
- `CHANGELOG.md` — изменения относительно оригинала.
- `LICENSE` — MIT с сохранённым копирайтом оригинального автора.
- `.gitignore` и `.gitattributes`.
- Исходники и готовая сборка контроллера в `Payload/`.
- Локальная история: оригинальный коммит Nick Farrow + коммиты форка.

## Важно перед публикацией

1. Прочитай README и docs целиком: это лицо репозитория.
2. Проверь, что в репозитории нет личных данных: `location.txt` уже в `.gitignore`, логи — тоже.
3. Убедись, что `Payload/DirectCombatController.exe` — свежая сборка из `Payload/DirectCombatController.cs`.
4. Реши, под каким аккаунтом публиковать, и настрой автора коммитов.

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
# в редакторе заменить pick на reword/exec или выполнить:
# exec git commit --amend --reset-author --no-edit
```

## Создание форка на GitHub

1. Открой [VeggieVampire/witcher-direct-combat](https://github.com/VeggieVampire/witcher-direct-combat).
2. Нажми **Fork** и создай форк в своём аккаунте.
3. В локальном репозитории переключи remote:

```bash
git remote rename origin upstream
git remote add origin https://github.com/<ваш-аккаунт>/witcher-direct-combat.git
git push -u origin main
```

Если форк не нужен, создай пустой репозиторий и используй его как `origin`. В README уже указан оригинальный автор и ссылка — это важно сохранить.

## После push

- Заполни описание репозитория и темы: `witcher`, `mod`, `gamepad`, `xinput`, `the-witcher-1`.
- Проверь, что GitHub корректно отображает README и ссылки на `docs/`.
- При желании сделай релиз: приложи ZIP с `Install.cmd`, `Payload/`, `README.md` и `LICENSE`.
- Не добавляй файлы игры и сохранения.

## Чего делать не нужно

- Не пушить `location.txt` и логи.
- Не менять копирайт Nick Farrow в `LICENSE` и `Payload/DirectCombatController.cs`.
- Не обещать в README того, чего нет: радиальное меню и вкладки инвентаря ещё не сделаны.
