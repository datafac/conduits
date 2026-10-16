: 
: imports source from RnD repo
:
@echo off

call :import DataFac.Conduits
call :import DataFac.Conduits.GrpcCommon
call :import DataFac.Conduits.GrpcClient
call :import DataFac.Conduits.GrpcServer

call :import DataFac.Conduits.HttpCommon
call :import DataFac.Conduits.HttpClient
call :import DataFac.Conduits.HttpServer

call :import DataFac.Conduits.ProtobufNet.Common
call :import DataFac.Conduits.ProtobufNet.Client
call :import DataFac.Conduits.ProtobufNet.Server

call :import DataFac.Conduits.Testing
call :import DataFac.Conduits.UnitTests

goto :eof

:import csFromDir
    echo Importing from %1 ...
    robocopy ..\rnd\%1 .\%1 *.cs /mir /xx /z
    set _rc=%errorlevel%
    echo     Returned: %_rc%
    goto :eof
