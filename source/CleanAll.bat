@ECHO OFF
pushd "%~dp0"
ECHO.
ECHO.
ECHO.
ECHO This script deletes all temporary build files in the .vs folder and the
ECHO BIN and OBJ folders contained in the following projects
ECHO.
ECHO Components\BusinessLib
ECHO Components\FilterTreeViewLib
ECHO FilterTreeView
ECHO.
REM Ask the user if hes really sure to continue beyond this point XXXXXXXX
set /p choice=Are you sure to continue (Y/N)?
if not '%choice%'=='Y' Goto EndOfBatch
REM Script does not continue unless user types 'Y' in upper case letter
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
ECHO.
ECHO Removing vs settings folder with *.sou file
ECHO.
RMDIR /S /Q .vs

ECHO.
ECHO Deleting BIN and OBJ Folders in BusinessLib
ECHO.
RMDIR /S /Q "Components\BusinessLib\bin"
RMDIR /S /Q "Components\BusinessLib\obj"

ECHO.
ECHO Deleting BIN and OBJ Folders in FilterTreeViewLib
ECHO.
RMDIR /S /Q "Components\FilterTreeViewLib\bin"
RMDIR /S /Q "Components\FilterTreeViewLib\obj"

ECHO.
ECHO Deleting BIN and OBJ Folders in FilterTreeView
ECHO.
RMDIR /S /Q ".\FilterTreeView\bin"
RMDIR /S /Q ".\FilterTreeView\obj"

PAUSE

:EndOfBatch
