# 1. ใช้ภาพจำลองที่มี .NET SDK เพื่อ Build โปรเจกต์
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# 2. ใช้ภาพจำลองสำหรับรันจริง (ขนาดเล็กกว่า)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# เปิด Port 8080 (Render บังคับใช้พอร์ตนี้)
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "Aimachine.dll"]