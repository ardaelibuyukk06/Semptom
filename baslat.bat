@echo off
echo =======================================================
echo Semptom Analiz Uygulamasi Baslatiliyor...
echo =======================================================
echo.
echo NOT: Bu bir .NET (C#) projesidir. PHP projeleri gibi XAMPP/WAMP 
echo icerisinde (htdocs) "localhost" yazarak calismaz!
echo Kendi ozel sunucusunu (Kestrel) baslatmasi gerekir.
echo.
echo Lutfen bekleyin, proje derleniyor ve calistiriliyor...
echo.

cd SemptomAnalizApp.Web
dotnet run

pause
