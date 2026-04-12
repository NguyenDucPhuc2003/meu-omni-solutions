# Prompt Migrate meu-omni Sang Modular DDD

## Prompt

Ban la expert solution architect va coding agent, nhiem vu cua ban la migrate toan bo he thong cu tu solution:

- `C:\Work\MeU Solutions\Project\meu-omni-solutions\meu-omni.sln`

sang kien truc moi theo solution:

- `C:\Work\MeU Solutions\Project\meu-omni-solutions\MeuOmni.Modular.sln`

Va source dich:

- `C:\Work\MeU Solutions\Project\meu-omni-solutions\src-modular`

## Muc tieu kien truc bat buoc

He thong dich phai dat cac dieu kien sau:

1. Kien truc theo `DDD + modular monolith`.
2. Moi module co `Domain`, `Application`, `Infrastructure`, `Api` rieng.
3. Moi module co `database rieng`, `DbContext rieng`, `connection string rieng`.
4. Khong duoc share database giua cac module.
5. Khong duoc tao foreign key xuyen module/database.
6. Khong duoc join truc tiep du lieu giua module.
7. Business rule phai nam trong `Domain`, khong nam trong controller.
8. `Bootstrap` chi dong vai tro composition root.
9. Migration phai theo huong incremental, khong duoc big-bang rewrite neu khong can.

## Tai lieu bat buoc phai doc truoc khi migrate

### Trong repo hien tai

- `C:\Work\MeU Solutions\Dang Doan Working Space\dang lam quan ly ban hang\qlbh\AGENT.md`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\docs\architecture.md`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\docs\modular-monolith-separate-databases.md`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\docs\checklist-status.md`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\docs\shift-login-open-shift-ddd.md`

### Tai lieu nghiep vu ngoai repo

- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\FRS_Phan_mem_quan_ly_ban_hang.docx`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\FRS_Phan_mem_quan_ly_ban_hang_v2_bo_sung_so_quy_ncc.docx`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\[MeU]_MasterCare_Chá»©c nÄƒng phÃ¢n há»‡ bÃ¡n hÃ ng cÆ¡ báº£n.xlsx`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\DanhSach_API_QLBH.xlsx`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\Checklist_thiet_ke_codebase_MasterCare.docx`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\Báº£ng Ma tráº­n TÆ°Æ¡ng tÃ¡c.docx`

### Hinh anh tham chieu

- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\images\erd.png`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\images\sÆ¡ Ä‘á»“ chá»©c nÄƒng.png`
- `C:\Work\MeU Solutions\Project\meu-omni-solutions\documents\images\sÆ¡ Ä‘á»“ luá»“ng.png`

## Dich vu/module muc tieu

### Module uu tien migrate truoc

- `SalesChannel`

Module nay phai tro thanh module trung tam cho:

- POS
- Hotline
- Facebook
- Zalo
- Website
- Marketplace

Tat ca order tu nhieu kenh phai quy ve model order chung trong `SalesChannel`.

### Module mo rong sau

- `SimpleCommerce`

Module nay chi la kenh online, khong duoc tro thanh order core thu hai.

Checkout cua `SimpleCommerce` ve sau phai dua order vao `SalesChannel`.

### Cac module tiep theo can tach

- `Catalog`
- `Inventory`
- `Customers`
- `AccessControl`
- `Auditing`
-  `Pricing`, `Payments`, `Promotions`, `Returns`

## Nhiem vu cu the khi migrate

1. Phan tich code hien tai trong `meu-omni.sln`.
2. Map code cu vao bounded context/module dich.
3. Xac dinh module ownership cho tung entity, repository, service, controller.
4. Di chuyen dan business rule tu layer ngang cu sang module doc moi.
5. Tai su dung code co gia tri, khong rewrite vo ich.
6. Moi module phai co:
   - project `Domain`
   - project `Application`
   - project `Infrastructure`
   - project `Api`
   - `DbContext` rieng
   - config DB rieng
7. Refactor theo tung dot nho, moi dot phai build duoc.
8. Sau moi dot, cap nhat tai lieu kien truc neu can.

## Quy tac mapping tu he thong cu

- `Sales` cu + `Shifts` cu uu tien gom vao `SalesChannel`
- `Catalog` cu tach thanh module `Catalog`
- `Inventory` cu tach thanh module `Inventory`
- `Customers` cu tach thanh module `Customers`
- `AccessControl` cu tach thanh module `AccessControl`
- `Auditing` cu tach thanh module `Auditing`

Neu mot use case dang dung repository/module khac:

- khong goi truc tiep DB module khac
- khong reference infrastructure module khac
- phai doi sang contract, event, hoac read model

## Cach lam viec mong muon

Ban phai lam viec theo chu ky sau:

1. Doc tai lieu va codebase.
2. Lap mapping source -> target.
3. Chon 1 nhom use case nho de migrate.
4. Implement code that su.
5. Build va test.
6. Bao cao ket qua, rui ro, va buoc tiep theo.

Khong dung lai o muc phan tich neu van co the code tiep.

## Dinh dang dau ra mong muon cho moi dot migrate

Sau moi dot, can bao cao:

- da migrate module/use case nao
- file nao duoc tao moi
- file nao duoc doi
- build/test co xanh khong
- con phu thuoc nao voi he thong cu
- buoc migrate tiep theo de nghi

## Thu tu migrate de xuat

1. `Sales + Shifts` -> `SalesChannel`
2. `Catalog`
3. `Inventory`
4. `Customers`
5. `AccessControl`
6. `Auditing`
7. `SimpleCommerce` checkout -> `SalesChannel`

## Dieu khong duoc lam

- khong share DB giua module
- khong dat business rule trong controller
- khong tham chieu truc tiep table/module khac bang join
- khong xoa solution cu neu chua migrate xong
- khong rewrite toan bo he thong mot lan

## Ket qua cuoi cung ky vong

He thong moi phai dat:

- chay tren `MeuOmni.Modular.sln`
- co the phat trien tiep theo `database per module`
- de mo rong cho `simple e-commerce`
- business rule ro rang, bounded context ro rang
- giam coupling so voi he thong cu

## Cach dung prompt nay

Ban co the dua nguyen prompt nay cho Copilot, Claude Code, Cursor, hoac coding agent khac, roi them lenh cu the o cuoi nhu:

- `Bat dau migrate Sales + Shifts sang SalesChannel`
- `Tao Catalog module va di chuyen product aggregate`
- `Refactor Inventory thanh module co database rieng`


