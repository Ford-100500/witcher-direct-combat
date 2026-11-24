Witcher Direct Combat 0.3.0 — установка

1. Полностью закрой The Witcher (процесс witcher.exe).
2. Запусти WitcherDirectCombatSetup.exe.
3. Проверь папку игры. Внутри должен быть System\witcher.exe.
   Если путь не определился, нажми «Обзор...» и выбери папку
   The Witcher Enhanced Edition.
4. Оставь галочку «Создать ярлык на рабочем столе».
5. Нажми «Установить».

Запуск:
- ярлык «The Witcher - Direct Combat» на рабочем столе, или
- DirectCombat\Play.cmd в папке игры.
В меню Steam выбери Play The Witcher.

Важно:
- Игра запускается только через Steam. Прямой witcher.exe в Steam-версии
  сразу закрывается — это не ошибка мода.
- Контроллер ждёт игру до 30 минут: Steam показывает меню запуска.
- Установщик и контроллер не подписаны. Антивирус может ругаться;
  добавь файлы в исключения или собери контроллер из исходников.
- Мод заменяет System\Scripts\testtools.lua. Если там был другой файл,
  он сохраняется как testtools.lua.wdc-backup.

Удаление:
- запусти Uninstall.cmd из этого архива и вставь путь к папке игры, или
- выполни в PowerShell:
  powershell -ExecutionPolicy Bypass -File .\Uninstall.ps1 -GameDir "путь к игре"

Ограничения и известные проблемы: docs\KNOWN-ISSUES.md
Карта управления: docs\CONTROLS.md
