# ProjectStars - Pokyny pro Claude

## Git workflow (POVINNÉ)
- Po každé dokončené úloze, která mění soubory:
  1. `git add .` (nebo jen změněné soubory)
  2. Commitni s **conventional commit** zprávou (feat:, fix:, refactor:, chore:, docs: atd.)
  3. `git push origin main`
- Vždy piš smysluplnou commit message, která popisuje, co se změnilo
- Nikdy nespouštěj git reset --hard nebo force push bez mého souhlasu

## Git & Remote
- Aktuální branch: main
- Remote: https://github.com/MartinForejt/ProjectTower.git

## Progress Tracking & Memory

- Po každé dokončené významné úloze (nebo na konci větší session) aktualizuj soubor **CHANGELOG.md** v kořeni projektu.
