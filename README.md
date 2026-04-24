# Aimachine API 🚀
โปรเจกต์ Backend สำหรับระบบ Company Profile และจัดการข้อมูลแผ่นดินไหว

## 🛠️ Tech Stack
- **Framework:** .NET 8 / ASP.NET Core
- **Database:** PostgreSQL (NeonDB)
- **Authentication:** JWT (JSON Web Token)
- **External Services:** Cloudinary (Image Management)

## 📁 Project Structure
- `Controllers/`: จัดการ API Endpoints
- `Models/`: โครงสร้าง Data Models
- `Services/`: Business Logic และการเชื่อมต่อ Cloudinary
- `Migrations/`: ไฟล์ประวัติการจัดการฐานข้อมูล

## 🚀 How to Run (Local)
1. Clone โปรเจกต์ไปที่เครื่อง
2. แก้ไข Connection String ใน `appsettings.json`
3. สั่ง `dotnet run` หรือกด F5 ใน Visual Studio
4. เข้าไปที่ `https://localhost:xxxx/swagger` เพื่อทดสอบ API

## 📝 Features
- [x] CRUD ระบบข้อมูลบริษัท (Partners, Services, Team)
- [x] ระบบยืนยันตัวตนด้วย JWT
- [x] ระบบจัดการรูปภาพผ่าน Cloudinary
- [x] ระบบประมวลผลข้อมูลแผ่นดินไหว
