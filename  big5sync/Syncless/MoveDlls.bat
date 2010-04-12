mkdir  "lib"
move /y "*.dll" "lib"

if exist de goto existsde
goto endde
:existsde
move /y "de" "lib"
:endde

if exist en goto existsen
goto enden
:existsen
move /y "en" "lib"
:enden

if exist es goto existses
goto endes
:existses
move /y "es" "lib"
:endes

if exist fr goto existsfr
goto endfr
:existsfr
move /y "fr" "lib"
:endfr

if exist it goto existsit
goto endit
:existsit
move /y "it" "lib"
:endit

if exist ja goto existsja
goto endja
:existsja
move /y "ja" "lib"
:endja

if exist ko goto existsko
goto endko
:existsko
move /y "ko" "lib"
:endko

if exist zh-Hans goto existszh-Hans
goto endzh-Hans
:existszh-Hans
move /y "zh-Hans" "lib"
:endzh-Hans

if exist zh-Hant goto existszh-Hant
goto endzh-Hant
:existszh-Hant
move /y "zh-Hant" "lib"
:endzh-Hant


