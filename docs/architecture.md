# meu-omni Architecture

## 1. Muc tieu kien truc

`meu-omni` duoc thiet ke theo huong:

- `DDD` de giu business rule trong domain
- `modular monolith` de tach module ro rang trong cung mot host
- `database per module` de moi module so huu du lieu va vong doi rieng

Kien truc nay phuc vu muc tieu truoc mat la `ban hang da kenh`, va mo rong ve sau voi module `simple e-commerce` ma khong pha vo loi nghiep vu hien tai.

## 2. Nguyen tac nen tang

1. Moi module so huu business va data cua chinh no.
2. Khong duoc join truc tiep du lieu giua cac module.
3. Khong tao foreign key xuyen module hoac xuyen database.
4. Giao tiep giua module qua contract, event, hoac read model.
5. Host trung tam chi lam composition root, khong chua business rule.
6. Business rule phai nam trong `Domain`, khong nam trong controller.
7. Migrations, schema, seed, va lifecycle DB duoc quan ly theo tung module.

## 3. Solution va cau truc tong the

Trong repo hien tai co 2 solution:

- solution cu: [meu-omni.sln](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/meu-omni.sln)
- solution moi theo huong modular: [MeuOmni.Modular.sln](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/MeuOmni.Modular.sln)

Huong moi duoc scaffold trong:

- [src-modular](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular)

Cau truc muc tieu:

```text
src-modular/
  MeuOmni.Bootstrap
  MeuOmni.BuildingBlocks
  Modules/
    SalesChannel/
      MeuOmni.Modules.SalesChannel.Domain
      MeuOmni.Modules.SalesChannel.Application
      MeuOmni.Modules.SalesChannel.Infrastructure
      MeuOmni.Modules.SalesChannel.Api
    SimpleCommerce/
      MeuOmni.Modules.SimpleCommerce.Domain
      MeuOmni.Modules.SimpleCommerce.Application
      MeuOmni.Modules.SimpleCommerce.Infrastructure
      MeuOmni.Modules.SimpleCommerce.Api
```

## 4. Vai tro tung layer

### MeuOmni.Bootstrap

Host trung tam, phu trach:

- start app
- DI registration
- middleware chung
- swagger
- module composition
- khoi tao DB cho tung module

Khong duoc chua business rule.

### MeuOmni.BuildingBlocks

Shared kernel toi thieu:

- `DomainException`
- `Entity`
- `AggregateRoot`
- `TenantAggregateRoot`
- `IModuleDefinition`

Chi giu nhung abstractions that su dung chung. Khong dua business cua module vao day.

### Domain

Chua:

- aggregate root
- entity
- enum
- repository contract
- invariant

Day la noi chot business rule.

### Application

Chua:

- command/query
- DTO
- application service
- orchestration

Day la tang dieu phoi use case, nhung khong chua persistence chi tiet.

### Infrastructure

Chua:

- DbContext rieng cua module
- repository implementation
- EF mapping
- persistence config
- module registration

### Api

Chua:

- controller hoac endpoint cua rieng module

Api layer chi nhan request, goi application service, va tra response.

## 5. Danh sach module muc tieu

### 5.1. SalesChannel

Day la module trung tam cua he thong.

Trach nhiem:

- tiep nhan don tu nhieu kenh
- normalize ve mot model don hang chung
- quan ly lifecycle order
- phan biet workflow theo channel

Channel du kien:

- POS
- Hotline
- Facebook
- Zalo
- Website
- Marketplace

Aggregate hien tai trong scaffold:

- `SalesOrder`
- `SalesOrderLine`

Code tham chieu:

- [SalesOrder.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Domain/Orders/Entities/SalesOrder.cs)
- [SalesOrderApplicationService.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Application/Orders/Services/SalesOrderApplicationService.cs)
- [SalesChannelDbContext.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SalesChannel/MeuOmni.Modules.SalesChannel.Infrastructure/Persistence/SalesChannelDbContext.cs)

### 5.2. SimpleCommerce

Module nay la kenh ban online don gian, khong phai he thong order core.

Trach nhiem:

- storefront
- public browsing
- cart/checkout o cac buoc sau
- dong vai tro kenh online cua `SalesChannel`

Nguyen tac quan trong:

- `SimpleCommerce` khong duoc tro thanh order core thu hai
- khi checkout thanh cong, du lieu phai di vao `SalesChannel`

Aggregate scaffold:

- `Storefront`

Code tham chieu:

- [Storefront.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SimpleCommerce/MeuOmni.Modules.SimpleCommerce.Domain/Storefronts/Entities/Storefront.cs)
- [SimpleCommerceDbContext.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/Modules/SimpleCommerce/MeuOmni.Modules.SimpleCommerce.Infrastructure/Persistence/SimpleCommerceDbContext.cs)

### 5.3. Cac module se tach tiep

Sau `SalesChannel` va `SimpleCommerce`, he thong se tiep tuc tach:

- `Catalog`
- `Inventory`
- `Customers`
- `AccessControl`
- `Auditing`
- co the them `Pricing`, `Payments`, `Promotions`, `Returns`

## 6. Chien luoc database per module

Moi module co:

- connection string rieng
- DbContext rieng
- migration rieng
- schema va vong doi du lieu rieng

Vi du trong host:

- `Modules:SalesChannel:Database:ConnectionString`
- `Modules:SimpleCommerce:Database:ConnectionString`

Code host doc config va khoi tao module:

- [Program.cs](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/MeuOmni.Bootstrap/Program.cs)

Y nghia:

- `SalesChannel` co the dung `qlbh_sales_channel`
- `SimpleCommerce` co the dung `qlbh_simple_commerce`

Ngay ca khi cung nam tren mot PostgreSQL server, van la hai database khac nhau.

## 7. Rule ve phu thuoc giua cac module

Rule mac dinh:

- `Bootstrap` duoc reference toi tat ca module
- `Api` cua module chi duoc reference `Application`
- `Application` chi duoc reference `Domain`
- `Infrastructure` duoc reference `Application` va `Domain`
- module nay khong duoc reference truc tiep `Infrastructure` cua module khac

Neu `SimpleCommerce` can tao order:

- khong ghi truc tiep vao DB cua `SalesChannel`
- khong reuse repository cua `SalesChannel`
- phai di qua application contract hoac integration message

## 8. Giao tiep giua module

Co 3 cach duoc phep:

### 8.1. Application contract

Dung khi can dong bo, pham vi noi bo host, va business don gian.

Vi du:

- `SimpleCommerce` goi contract tao `SalesOrder`

### 8.2. Integration event

Dung khi can tach coupling hon, nhat la voi:

- inventory reservation
- payment confirmation
- audit trail

### 8.3. Read model

Dung khi can man hinh tong hop du lieu tu nhieu module, nhung van khong duoc join truc tiep DB.

## 9. Business map cho ban hang da kenh

### Core order model

Moi don hang, du den tu kenh nao, deu phai quy ve `SalesOrder`.

Khac biet giua cac kenh nam o:

- metadata
- workflow
- cach xac nhan
- cach thanh toan
- cach giao hang

Khong duoc moi kenh tao mot order aggregate rieng neu cung la mot su kien ban hang.

### POS

Can bo sung tiep:

- open shift / close shift
- thu ngan
- register
- cash reconciliation

Phan nay se duoc dua vao `SalesChannel` hoac tach `RetailOperations` neu nghiep vu van hanh qua lon.

### Online channels

`SimpleCommerce` la online channel dau tien.

Ve sau co the them:

- marketplace connector
- social inbox order intake
- chatbot order intake

Nhung tat ca deu phai day order ve `SalesChannel`.

## 10. Trang thai hien tai

Da scaffold xong:

- host modular
- `BuildingBlocks`
- `SalesChannel`
- `SimpleCommerce`
- config DB rieng tung module

Da build thanh cong:

- `dotnet build MeuOmni.Modular.sln`

## 11. Lo trinh migrate tu code hien tai

### Phase 1

- giu `meu-omni.sln` de he thong cu van chay
- phat trien kien truc moi trong `MeuOmni.Modular.sln`

### Phase 2

- migrate `Sales`
- migrate `Shifts`
- hop nhat thanh `SalesChannel`

### Phase 3

- tach `Catalog`
- tach `Inventory`
- tach `Customers`

### Phase 4

- tach `AccessControl`
- tach `Auditing`
- them integration event/outbox

### Phase 5

- mo rong `SimpleCommerce`
- noi checkout vao `SalesChannel`

## 12. Tai lieu lien quan

- chi tiet scaffold va migration note: [modular-monolith-separate-databases.md](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/docs/modular-monolith-separate-databases.md)


