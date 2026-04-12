# meu-omni Modular Monolith With Separate Databases

## Muc tieu

Tai lieu nay chot huong tai cau truc `meu-omni` theo:

- `DDD` giu business rule nam trong domain
- `modular monolith` de tach module ro rang trong cung mot host
- `database per module` dung nghia: moi module co ket noi va DbContext rieng, khong share bang

## Nguyen tac kien truc

1. Moi module so huu nghiep vu va du lieu cua chinh no.
2. Khong join truc tiep du lieu giua cac module.
3. Khong tao foreign key xuyen database/module.
4. Giao tiep giua module qua:
   - application contract
   - integration event/outbox
   - read model phu tro neu can
5. Host trung tam chi lam:
   - composition root
   - auth/middleware
   - swagger
   - khoi tao module

## Scaffold da them

Solution moi: [MeuOmni.Modular.sln](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/MeuOmni.Modular.sln)

Host:

- [MeuOmni.Bootstrap](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/MeuOmni.Bootstrap/Program.cs)

Shared building blocks:

- [MeuOmni.BuildingBlocks](C:/Work/MeU%20Solutions/Dang%20Doan%20Working%20Space/dang%20lam%20quan%20ly%20ban%20hang/qlbh/meu-omni/src-modular/MeuOmni.BuildingBlocks)

Module di truoc:

- `SalesChannel`
  - Domain/Application/Infrastructure/Api
  - database rieng qua `SalesChannelDbContext`

Module de mo rong sau:

- `SimpleCommerce`
  - Domain/Application/Infrastructure/Api
  - database rieng qua `SimpleCommerceDbContext`

## Database-per-module

Host doc config tu:

```json
"Modules": {
  "SalesChannel": {
    "Database": {
      "ConnectionString": "..."
    }
  },
  "SimpleCommerce": {
    "Database": {
      "ConnectionString": "..."
    }
  }
}
```

Dieu nay cho phep:

- `SalesChannel` dung DB `qlbh_sales_channel`
- `SimpleCommerce` dung DB `qlbh_simple_commerce`

Ngay ca khi cung mot PostgreSQL server, van la hai database tach biet.

## Mapping business

### SalesChannel

Day la module trung tam cho ban hang da kenh:

- POS
- Hotline
- Facebook
- Zalo
- Website
- Marketplace

Loi chung la `SalesOrder`.

Moi kenh khac nhau ve workflow, nhung khong duoc tao he model don hang rieng.

### SimpleCommerce

Module nay khong duoc giu don hang that rieng ve lau dai.

Nhiem vu chinh:

- storefront
- public catalog
- cart/checkout sau nay
- dong vai tro mot kenh vao `SalesChannel`

Ve sau, checkout cua `SimpleCommerce` phai dua ve `SalesChannel` de tao `SalesOrder`.

## Huong migrate tu meu-omni hien tai

1. Giu source cu de van chay duoc.
2. Dua module moi vao `src-modular`.
3. Chuyen truoc business `Sales + Shifts` thanh `SalesChannel`.
4. Tach `Catalog`, `Inventory`, `Customers`, `AccessControl`, `Auditing` thanh module rieng o cac dot tiep theo.
5. Chi khi contract on dinh moi cat bo layer ngang cu.

## Buoc tiep theo de lam tiep

1. Di chuyen `Shifts` vao trong `SalesChannel` hoac tach `RetailOperations`.
2. Tao `Catalog` module rieng va doi `SalesChannel` tu luu snapshot san pham sang goi contract.
3. Tao `Inventory` module rieng va them outbox cho xuat kho theo order.
4. Tao `AccessControl` module rieng de host dung auth/permission theo module.

