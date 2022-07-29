@ECHO OFF

CHCP 65001

SET SIDE=

:LOOP
	SET /P SIDE="座席を選択してください（E:東 or W:西 or S:南 or N:北）："

	IF /i NOT %SIDE%==E IF /i NOT %SIDE%==W IF /i NOT %SIDE%==S IF /i NOT %SIDE%==N GOTO :LOOP

START DoljaraApp.exe %SIDE%