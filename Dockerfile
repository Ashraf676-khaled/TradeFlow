# المرحلة الأولى: بناء المشروع (Build Stage)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# نسخ ملفات الإعدادات والـ Packages لضمان كفاءة التخزين المؤقت (Caching)
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["TradeFlow.slnx", "./"]

# نسخ باقي المشاريع بناءً على مساراتها الحقيقية في TradeFlow
COPY ["src/TradeFlow.Api/TradeFlow.Api.csproj", "src/TradeFlow.Api/"]
COPY ["src/TradeFlow.Application/TradeFlow.Application.csproj", "src/TradeFlow.Application/"]
COPY ["src/TradeFlow.Domain/TradeFlow.Domain.csproj", "src/TradeFlow.Domain/"]
COPY ["src/TradeFlow.Infrastructure/TradeFlow.Infrastructure.csproj", "src/TradeFlow.Infrastructure/"]

# استعادة الحزم (Restore NuGet Packages)
RUN dotnet restore "TradeFlow.slnx"

# نسخ كل الكود المصدري وبناء المشروع بنظام Release
COPY . .
WORKDIR "/src/src/TradeFlow.Api"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# المرحلة الثانية: التشغيل النهائي (Runtime Stage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TradeFlow.Api.dll"]