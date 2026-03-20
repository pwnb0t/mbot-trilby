@echo off
setlocal

rem Repo layout expected:
rem E:\g\<mbot repo>
rem E:\g\<mbot-trilby repo>

set SCRIPT_DIR=%~dp0
for %%I in ("%SCRIPT_DIR%..") do set TRILBY_REPO=%%~fI
for %%I in ("%TRILBY_REPO%..\..") do set WORK_ROOT=%%~fI
for %%I in ("%TRILBY_REPO%") do set TRILBY_REPO_NAME=%%~nxI

set INPUT_SPEC=
set INPUT_SPEC_DOCKER=
for /d %%I in ("%WORK_ROOT%\*") do (
  if not defined INPUT_SPEC if exist "%%~fI\openapi\trilby.v1.yaml" (
    set INPUT_SPEC=%%~fI\openapi\trilby.v1.yaml
    set INPUT_SPEC_DOCKER=/work/%%~nxI/openapi/trilby.v1.yaml
  )
)

if not defined INPUT_SPEC (
  echo Missing input spec: could not find a sibling openapi\trilby.v1.yaml under %WORK_ROOT%
  exit /b 1
)

set OUTPUT_DIR=/work/%TRILBY_REPO_NAME%/mbot-trilby/src/Generated/TrilbyApi

echo Generating C# SDK from %INPUT_SPEC%
docker run --rm ^
  -v "%WORK_ROOT%:/work" ^
  openapitools/openapi-generator-cli generate ^
  -i %INPUT_SPEC_DOCKER% ^
  -g csharp ^
  -o %OUTPUT_DIR% ^
  --additional-properties=packageName=TrilbyApi,packageVersion=0.0.1,targetFramework=net8.0

if errorlevel 1 (
  echo SDK generation failed.
  exit /b 1
)

echo SDK generation complete.
endlocal
