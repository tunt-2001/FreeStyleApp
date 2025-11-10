# Hướng dẫn sử dụng FeatureGroup và cách thêm Controller/Action mới

## Cách hệ thống load Controllers và Actions

### 1. Tự động quét Controllers
Hệ thống sử dụng `IActionDescriptorCollectionProvider` để **TỰ ĐỘNG** quét tất cả các Controller trong project. Điều này có nghĩa là:

- ✅ **Bất kỳ Controller mới nào** bạn tạo trong thư mục `Controllers/` sẽ **TỰ ĐỘNG** xuất hiện trong dropdown
- ✅ **Bất kỳ Action mới nào** bạn thêm vào Controller cũng sẽ **TỰ ĐỘNG** xuất hiện trong dropdown Action
- ✅ **Không cần cấu hình thêm** - hệ thống tự động phát hiện

### 2. Cách thêm Controller/Action mới

#### Ví dụ: Tạo Controller mới

```csharp
// Controllers/ProductController.cs
[Authorize]
public class ProductController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Create()
    {
        return View();
    }
    
    public IActionResult Edit(string id)
    {
        return View();
    }
}
```

**Kết quả:**
- Controller "Product" sẽ **TỰ ĐỘNG** xuất hiện trong dropdown Controller
- Actions: Index, Create, Edit sẽ **TỰ ĐỘNG** xuất hiện trong dropdown Action khi chọn Product

#### Ví dụ: Thêm Action mới vào Controller hiện có

```csharp
// Controllers/UserController.cs
public class UserController : Controller
{
    // ... existing code ...
    
    // Thêm action mới
    public IActionResult Export()
    {
        return View();
    }
}
```

**Kết quả:**
- Action "Export" sẽ **TỰ ĐỘNG** xuất hiện trong dropdown Action khi chọn User controller

### 3. Các Action bị loại bỏ tự động

Hệ thống tự động loại bỏ các action không phù hợp cho menu:

- ❌ Các action trả về JSON (Get*, Save*, Update*, Delete*, Export*, Send*, Assign*)
- ❌ Các action helper (TestDatabaseConnection, ClearCache, ChangePassword, AccessDenied)
- ❌ Account controller (login/logout)

### 4. Lưu ý

1. **Controller phải kế thừa từ `Controller`** hoặc có `[Controller]` attribute
2. **Action phải là public** và trả về `IActionResult` hoặc `Task<IActionResult>`
3. **Action phải được đăng ký trong routing** (mặc định tất cả actions trong Controller đều được đăng ký)
4. **Không cần restart server** - chỉ cần refresh trang "Cấu hình Menu" là thấy Controller/Action mới

### 5. Ví dụ đầy đủ

```csharp
// 1. Tạo Controller mới
namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IAppDbContext _context;
        
        public OrderController(IAppDbContext context)
        {
            _context = context;
        }
        
        // Action này sẽ TỰ ĐỘNG xuất hiện trong dropdown
        public IActionResult Index()
        {
            return View();
        }
        
        // Action này cũng sẽ TỰ ĐỘNG xuất hiện
        public IActionResult Create()
        {
            return View();
        }
        
        // Action này sẽ BỊ LOẠI BỎ (vì trả về JSON)
        [HttpPost]
        public IActionResult Save([FromBody] OrderDto dto)
        {
            return Json(new { success = true });
        }
    }
}
```

**Sau khi tạo Controller trên:**
1. Build project: `dotnet build`
2. Refresh trang "Cấu hình Menu"
3. Click "Thêm chức năng"
4. Dropdown Controller sẽ có "Order"
5. Dropdown Action sẽ có "Index" và "Create" (không có "Save")

## Tóm tắt

- ✅ **Tự động**: Không cần cấu hình, chỉ cần tạo Controller/Action mới
- ✅ **Real-time**: Refresh trang là thấy ngay
- ✅ **Thông minh**: Tự động loại bỏ các action không phù hợp
- ✅ **Dễ dàng**: Chỉ cần tuân thủ convention của ASP.NET Core MVC

