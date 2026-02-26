@echo off
setlocal

rem Repo layout expected:
rem E:\g\ownbot
rem E:\g\ownbotsidekick

set SCRIPT_DIR=%~dp0
for %%I in ("%SCRIPT_DIR%..") do set SIDEKICK_REPO=%%~fI
for %%I in ("%SIDEKICK_REPO%..\..") do set WORK_ROOT=%%~fI

set INPUT_SPEC=%WORK_ROOT%\ownbot\openapi\sidekick.v1.yaml
set OUTPUT_DIR=/work/ownbotsidekick/ownbotsidekick/src/Generated/SidekickApi

if not exist "%INPUT_SPEC%" (
  echo Missing input spec: %INPUT_SPEC%
  exit /b 1
)

echo Generating C# SDK from %INPUT_SPEC%
docker run --rm ^
  -v "%WORK_ROOT%:/work" ^
  openapitools/openapi-generator-cli generate ^
  -i /work/ownbot/openapi/sidekick.v1.yaml ^
  -g csharp ^
  -o %OUTPUT_DIR% ^
  --additional-properties=packageName=SidekickApi,packageVersion=0.0.1,targetFramework=net8.0

if errorlevel 1 (
  echo SDK generation failed.
  exit /b 1
)

echo SDK generation complete.
endlocal
