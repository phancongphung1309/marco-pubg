# Mouse Studio

**Tiếng Việt** | [English](README.en.md)

Công cụ giảm giật (recoil) cho **PUBG: Battlegrounds** trên Windows. Khi bạn bắn, ứng dụng kéo chuột xuống để bù độ giật dọc của súng, giúp tâm ngắm giữ đúng mục tiêu mà bạn không cần tự ghì chuột.

### Không có chuột Logitech? Đừng lo, dùng ứng dụng này.

Hầu hết macro giảm giật là script Lua cho Logitech G Hub, nên chỉ chạy được với chuột Logitech. Mouse Studio chạy cùng loại script Lua đó cho **mọi loại chuột**: Razer, SteelSeries, chuột văn phòng không thương hiệu, chuột nào cũng được. Không cần G Hub, không cần driver đặc biệt, không cần phần mềm thêm.

Ứng dụng không đọc hay thay đổi game: nó chỉ di chuyển con trỏ, giống như một macro G Hub. Các chuyển động đến từ script Lua nên có thể tinh chỉnh. Script G Hub chuyển sang rất dễ vì các hàm có cùng tên (`MoveMouseRelative`, `IsMouseButtonPressed`, `Sleep`, …).

> Chuột nào cũng dùng được. Mặc định việc kéo xuống được kích hoạt bằng nút bên Forward (đa số chuột gaming đều có), nhưng bạn có thể chọn phím hoặc nút khác, hoặc một tổ hợp như "giữ chuột phải 1 giây, rồi nhấn chuột trái".

### Cách hoạt động

Giữ phím kích hoạt (mặc định là nút bên **Forward** của chuột, hoặc phím hay tổ hợp do bạn chọn) để bắn và kéo con trỏ xuống theo một bước cố định với tốc độ cố định, tất cả do bạn cài đặt. Lưu tối đa 12 hồ sơ (cho các súng, ống ngắm hoặc độ nhạy khác nhau) và chuyển đổi giữa chúng ngay trong game bằng **F1 đến F12**.

> **Cảnh báo:** dùng macro hoặc script giảm giật là vi phạm điều khoản dịch vụ của PUBG. Tài khoản của bạn có thể bị khóa. Tự chịu rủi ro khi sử dụng.

> **Được làm 100% bằng AI.** Mọi dòng code trong dự án này đều do AI viết thông qua vibe coding. Xem [Về dự án này](#về-dự-án-này).

![Mouse Studio với một hồ sơ Movement được chọn và log script trong console](app_screenshot.png)

## Bắt đầu nhanh (không cần cài đặt)

1. Tải [Marco-Pubg.zip](https://github.com/phancongphung1309/marco-pubg/raw/main/Marco-Pubg.zip).
2. Giải nén ở bất kỳ đâu, ví dụ ra Desktop. Giữ tất cả các tệp cùng nhau: `MouseStudio.exe` cần thư mục `Scripts` nằm cạnh nó.
3. Mở thư mục `Marco-Pubg` và nhấp đúp **`MouseStudio.exe`**.
4. Bấm **Yes** khi Windows hỏi quyền quản trị (administrator).
5. Chọn một hồ sơ (hoặc nhấn **F1 đến F12**) rồi vào game. Giữ phím kích hoạt (mặc định là Forward) để bắn.

Nếu Windows hiện "Windows protected your PC", bấm **More info**, rồi **Run anyway**. Ứng dụng chưa được ký số nên Windows cảnh báo.

Không cần gì thêm: .NET đã được tích hợp sẵn trong tệp thực thi. Tiếp theo xem [Cách sử dụng](#cách-sử-dụng).

## Build từ mã nguồn

### Yêu cầu

- Windows 10 hoặc 11 (64-bit)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), chỉ cần khi build từ mã nguồn
- Quyền quản trị: ứng dụng yêu cầu quyền này khi khởi động (`app.manifest`), để các hook vẫn hoạt động khi cửa sổ game chạy với quyền quản trị đang được focus

### Build và chạy

```powershell
dotnet build MouseStudio.csproj
dotnet run --project MouseStudio.csproj
```

Các script trong `Scripts/` được sao chép cạnh tệp thực thi khi build.

### Xuất bản thành một tệp thực thi

```powershell
dotnet publish MouseStudio.csproj -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true -p:DebugType=None -o Marco-Pubg
```

Lệnh này ghi `MouseStudio.exe` và thư mục `Scripts` vào `Marco-Pubg/`. Hãy phân phối cả thư mục.

### Kiểm thử

```powershell
dotnet test tests/MouseStudio.Tests
```

Các bài test xUnit bao gồm dữ liệu hồ sơ, kiểm tra tên, việc lưu, tải, nhập, xuất hồ sơ, quy tắc phím nóng và thời gian của tổ hợp.

## Cách sử dụng

### 1. Khởi động ứng dụng

Chạy `MouseStudio.exe` và chấp nhận yêu cầu quyền quản trị. Giữ thư mục `Scripts` cạnh tệp thực thi: ứng dụng chạy các script trong đó.

Lần đầu khởi động, ứng dụng tạo ba hồ sơ: **F1 - 5mm**, **F2 - 7mm** và **F3 - RPD**. Dòng trạng thái ở dưới cùng chuyển sang màu xanh và hiện `Movement đang chạy · movement.lua` khi script đang chạy. Console bên dưới cho biết ứng dụng và script đang làm gì. Không có nút Start: script chạy suốt khi ứng dụng còn mở.

Ứng dụng mặc định dùng tiếng Việt. Bấm nút **EN / VI** cạnh nút **Ủng hộ** (Donate) để chuyển giữa tiếng Việt và tiếng Anh; lựa chọn được lưu trong `profiles.json`.

### 2. Chọn phím kích hoạt

Thẻ **Hotkey** ở trên cùng quyết định thứ gì khởi chạy di chuyển. Cài đặt này dùng chung cho mọi hồ sơ và được tự động lưu. Chọn chế độ trong danh sách thả xuống của thẻ:

- **Một phím** (mặc định): di chuyển chạy khi đang giữ một phím. Bấm nút dưới **Phím cần giữ**, rồi nhấn phím hoặc nút chuột muốn dùng. Mặc định là **Chuột Forward**.
  - Được phép: mọi phím trên bàn phím, hoặc nút giữa, back hay forward của chuột.
- **Tổ hợp**: di chuyển chạy sau một chuỗi ba bước:

  | Bước | Ý nghĩa | Mặc định |
  | --- | --- | --- |
  | 1 · Giữ | Phím hoặc nút chuột cần giữ trước | Chuột phải |
  | 2 · Trong ít nhất | Thời gian phải giữ bước 1, tính bằng ms (0 đến 60000) | 1000 |
  | 3 · Rồi nhấn | Phím hoặc nút chuột bắt đầu di chuyển | Chuột trái |

  Ví dụ với giá trị mặc định: giữ chuột phải để ngắm (ADS), chờ một giây, rồi nhấn chuột trái để bắn kèm kéo xuống. Di chuyển chạy khi đang giữ bước 3, và dừng ngay khi thả bước 1 **hoặc** bước 3. Nhấn bước 3 quá sớm, hoặc không giữ bước 1, sẽ không có gì xảy ra. Khi vẫn đang giữ bước 1, bạn có thể thả rồi nhấn lại bước 3 mà không phải chờ lại.

  Bước 1 và bước 3 có thể là bất kỳ phím hay nút chuột nào, kể cả chuột trái và chuột phải, nhưng không được trùng nhau.

Để đổi một phím, bấm vào nút của nó: nút hiện `Nhấn một phím…` cho đến khi bạn nhấn. **Esc**, bấm chỗ khác, hoặc chuyển sang cửa sổ khác sẽ hủy. Console ghi lại mỗi lần thay đổi.

> Phím bàn phím có tác dụng ở mọi nơi, kể cả khi bạn gõ trong các ô của Mouse Studio: nếu chọn **V** làm phím kích hoạt, gõ chữ V ở đó cũng sẽ chạy di chuyển.

### 3. Dùng Movement

1. Đặt các giá trị trong mục Cài đặt di chuyển:

   | Cài đặt | Ý nghĩa | Mặc định |
   | --- | --- | --- |
   | Di chuyển ngang | Số pixel di chuyển sang phải mỗi bước (giá trị âm sang trái). Có thể dùng số lẻ như `0.5`: chúng được cộng dồn qua các bước. | 0 |
   | Di chuyển dọc | Số pixel kéo xuống mỗi bước (giá trị âm kéo lên) | 5 |
   | Chu kỳ | Thời gian giữa hai bước, tính bằng ms (ít nhất 1) | 10 |
   | Thời gian kích hoạt | Mỗi lần kéo kéo dài bao lâu, tính bằng ms (ít nhất 1) | 5000 |
   | Độ trễ lặp lại | Thời gian nghỉ trước lần kéo tiếp theo, tính bằng ms (0 trở lên) | 1000 |

   Thay đổi được lưu nửa giây sau khi bạn ngừng gõ, và script tải lại với giá trị mới. Giá trị không hợp lệ (để trống, hoặc ngoài phạm vi) sẽ không được lưu: console cho biết lý do, và các giá trị hợp lệ gần nhất vẫn được dùng.

2. **Giữ phím kích hoạt** (xem [Chọn phím kích hoạt](#2-chọn-phím-kích-hoạt)). Khi đang giữ, ứng dụng:
   1. giữ chuột trái và di chuyển con trỏ một bước sau mỗi Chu kỳ, trong suốt Thời gian kích hoạt;
   2. nhả chuột trái và chờ Độ trễ lặp lại;
   3. bắt đầu lại từ bước 1.

3. **Nhả phím kích hoạt** để dừng ngay lập tức. Chuột trái cũng được nhả.

### 4. Quản lý hồ sơ

Hồ sơ giúp bạn lưu nhiều bộ giá trị Movement và chuyển đổi nhanh giữa chúng. Chúng nằm ở đầu bảng cài đặt:

- **Mới**: thêm một hồ sơ (tối đa 12). Hồ sơ mới bắt đầu với giá trị mặc định.
- **Đổi tên**: đổi tên hồ sơ đang chọn. Tên không được trùng nhau.
- **Xóa**: xóa hồ sơ đang chọn, sau khi xác nhận. Không thể xóa hồ sơ cuối cùng.
- **Danh sách thả xuống**: chọn hồ sơ để dùng.
- **F1 đến F12**: tải hồ sơ thứ 1 đến thứ 12 trong danh sách, từ bất kỳ đâu, kể cả khi game đang được focus. Thứ tự là thứ tự trong danh sách.

Để sao lưu hồ sơ hoặc chuyển sang máy khác:

- **Xuất** lưu toàn bộ hồ sơ ra một tệp `.json`.
- **Nhập** tải một tệp `.json` được tạo bằng Xuất. Tệp được kiểm tra trước; nếu hợp lệ, ứng dụng sẽ hỏi trước khi **thay thế toàn bộ hồ sơ hiện tại** bằng các hồ sơ trong tệp.

Hồ sơ được lưu trong `profiles.json` cạnh tệp thực thi.

### 5. Chơi với overlay

Bấm **Hiện Overlay** trước khi vào game:

- Một hộp nhỏ luôn nằm trên cùng hiển thị hồ sơ hiện tại.
- Cửa sổ chính được ẩn xuống khay hệ thống (tray).
- Kéo hộp bằng chuột trái để di chuyển. Vị trí của nó được ghi nhớ.
- Nhấp đúp vào hộp, hoặc bấm biểu tượng ở khay, để mở lại cửa sổ chính.
- Nhấp phải biểu tượng ở khay để chọn **Mở Mouse Studio**, **Ẩn Overlay** và **Thoát**.

Bấm **Ẩn Overlay** để đóng hộp.

### Cấu hình của tác giả

Các hồ sơ mặc định (**F1 - 5mm**, **F2 - 7mm**, **F3 - RPD**) được tinh chỉnh trong game với cấu hình này. Mức kéo xuống tính bằng pixel, nên tâm ngắm di chuyển bao xa còn tùy vào DPI và độ nhạy trong game của bạn. Nếu cấu hình của bạn khác, hãy bắt đầu từ các giá trị này và chỉnh Di chuyển dọc cho đến khi hết giật.

- **DPI chuột:** 1600
- **Cài đặt PUBG** (Cài đặt → Điều khiển → Chuột):

  | Cài đặt | Giá trị |
  | --- | --- |
  | Hoán đổi chuột (Invert Mouse) | Tắt |
  | Độ nhạy chung (General Sensitivity) | 45 |
  | Hệ số nhạy dọc (Vertical Sensitivity Multiplier) | 1 |
  | Độ nhạy Nhắm (Aim Sensitivity) | 45 |
  | Độ nhạy Nhắm (ADS Sensitivity) | 45 |
  | Độ Nhạy cho tất cả các loại Scope (Universal Sensitivity for All Scopes) | Bật |
  | Độ nhạy Nòng nhắm (Scoping Sensitivity) | 45 |

Game dùng cùng tên "Độ nhạy Nhắm" cho cả Aim và ADS; trong menu, Aim nằm trên ADS. Khi bật **Độ Nhạy cho tất cả các loại Scope**, mọi ống ngắm dùng chung một Độ nhạy Nòng nhắm, nên một hồ sơ hoạt động giống nhau với mọi ống ngắm.

### Khắc phục sự cố

- **Không có gì xảy ra trong game**: đảm bảo ứng dụng chạy với quyền quản trị và dòng trạng thái màu xanh. Kiểm tra console xem có lỗi script không.
- **Tổ hợp không chạy**: giữ bước 1 ít nhất bằng thời gian đặt ở bước 2 rồi mới nhấn bước 3, và tiếp tục giữ bước 1 trong khi bắn.
- **`Không tìm thấy script Lua`**: thư mục `Scripts` không nằm cạnh `MouseStudio.exe`. Sao chép lại từ thư mục đã xuất bản.
- **Một giá trị không lưu được**: đọc dòng `Chưa lưu:` trong console; nó cho biết trường nào và phạm vi cho phép.
- **Sai hướng**: dùng giá trị âm cho Di chuyển ngang (sang trái) hoặc Di chuyển dọc (lên trên).

## Script

Một script định nghĩa `OnEvent(event, arg)`, được Mouse Studio gọi với:

| Sự kiện | `arg` |
| --- | --- |
| `PROFILE_ACTIVATED` | `0`, khi script bắt đầu |
| `HOTKEY_PRESSED` / `HOTKEY_RELEASED` | `0`, khi phím kích hoạt đặt trong ứng dụng (một phím hoặc tổ hợp) bắt đầu hoặc dừng |
| `MOUSE_BUTTON_PRESSED` / `MOUSE_BUTTON_RELEASED` | Nút chuột: 1 trái, 2 phải, 3 giữa, 4 back, 5 forward |
| `KEY_PRESSED` | Mã phím ảo (F1 đến F12 được dành cho phím tắt hồ sơ) |

Chỉ tính nút bấm thật: các cú click do script tự gửi không tạo sự kiện chuột, và `IsMouseButtonPressed` bỏ qua chúng, giống như script Logitech.

Các cài đặt của hồ sơ được gán thành biến toàn cục trước khi script chạy: `MOVE_X`, `MOVE_Y`, `INTERVAL`, `ACTIVE_DURATION` và `REPEAT_DELAY`.

Các hàm script có thể dùng (`Core/Lua/LuaApi.cs`):

| Hàm | Mô tả |
| --- | --- |
| `MoveMouseRelative(x, y)` | Di chuyển con trỏ; phần lẻ được cộng sang lần gọi sau |
| `PressMouseButton(b)` / `ReleaseMouseButton(b)` | Giữ hoặc nhả nút 1 (trái) hoặc 2 (phải) |
| `ClickMouseButton(b)` | Click nút 1 hoặc 2 |
| `IsMouseButtonPressed(b)` | Nút 1, 2, 3, 4 hoặc 5 có đang được giữ thật không |
| `IsHotkeyPressed()` | Phím kích hoạt đặt trong ứng dụng có đang được giữ không |
| `IsKeyLockOn(key)` | `"capslock"`, `"numlock"` hoặc `"scrolllock"` |
| `Sleep(ms)` | Chờ; kết thúc sớm khi script bị dừng |
| `GetTickCount()` | Số mili giây kể từ khi Windows khởi động |
| `OutputLogMessage(text)` | Ghi ra console của ứng dụng |

Ứng dụng chạy `Scripts/movement.lua`. Script này phản hồi `HOTKEY_PRESSED` và lặp khi `IsHotkeyPressed()`, nên nó theo bất kỳ phím kích hoạt nào được đặt trong ứng dụng.

## Cấu trúc dự án

```
MainWindow.xaml(.cs)       Cửa sổ chính: hồ sơ, cài đặt, console
OverlayWindow.xaml(.cs)    Overlay trạng thái luôn nằm trên cùng
ProfileNameDialog.xaml     Hộp thoại tạo / đổi tên hồ sơ
DonateWindow.xaml(.cs)     Cửa sổ mã QR ủng hộ
Core/Input/                Hook chuột và bàn phím toàn cục, theo dõi phím kích hoạt, điều khiển chuột
Core/Lua/                  Lua engine (NLua) và API cho script
Core/Profiles/             Mô hình hồ sơ, phím nóng và tổ hợp, lưu trữ profiles.json
Core/Localization/         Văn bản giao diện tiếng Anh và tiếng Việt
Scripts/                   Các script Lua được sao chép cạnh tệp thực thi
tests/MouseStudio.Tests/   Các bài test xUnit
```

## Về dự án này

Dự án này được làm **100% bằng AI, bằng vibe coding**. Không dòng code nào được gõ tay: tác giả mô tả những gì mình muốn bằng lời, thử kết quả, và yêu cầu AI sửa đổi cho đến khi ứng dụng hoạt động đúng như mong muốn.

AI đã viết toàn bộ:

- ứng dụng WPF: các cửa sổ, giao diện tối, overlay và biểu tượng khay;
- hook chuột và bàn phím cấp thấp của Windows và phần điều khiển chuột;
- Lua engine và API script, mô phỏng theo script Logitech G Hub;
- phần lưu trữ hồ sơ, có nhập và xuất;
- giao diện tiếng Anh và tiếng Việt;
- script Lua kéo xuống;
- các bài unit test;
- README này.

Phần của tác giả là ý tưởng, yêu cầu, thử nghiệm trong game, tinh chỉnh giá trị, và quyết định giữ lại những gì.

### Điều này có ý nghĩa gì với bạn

- **Ứng dụng hoạt động, nhưng chưa được một lập trình viên xem xét từng dòng.** Có thể còn chỗ chưa hoàn thiện; hãy báo lỗi ở mục [issues](https://github.com/phancongphung1309/marco-pubg/issues).
- **Các hồ sơ mặc định có thể không khớp với độ nhạy chuột hoặc ống ngắm của bạn.** Chúng được tinh chỉnh với [cấu hình của tác giả](#cấu-hình-của-tác-giả). Hãy chỉnh lại giá trị, hoặc tạo hồ sơ riêng.
- **Hãy đọc code trước khi tin dùng**, như với bất kỳ công cụ nào chạy với quyền quản trị và hook chuột, bàn phím của bạn. Toàn bộ code đều ở đây, và nó ngắn.

Đây cũng là một ví dụ về những gì vibe coding có thể làm ngày nay: một ứng dụng desktop Windows hoạt động được với hook native, một scripting engine và test, được làm mà không cần tự tay viết code.

## Ủng hộ

Nếu ứng dụng hữu ích với bạn, bạn có thể ủng hộ tác giả: bấm **Ủng hộ** (Donate) trong ứng dụng để hiện mã QR (VietQR / Napas 247).
