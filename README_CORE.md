# Integrated with Backend NGX-ADMIN template

## Before solution start you need to:

1) setup connection string in `appsettings.json`

```
"ConnectionStrings": {
    "localDb": "<connection_string>"
  }
```

2) add api url to `<ECommerce/IoT>.WebApiCore/ClientCode/src/environments/environment.ts`

```
export const environment = {
  production: false,
  apiUrl: 'https://localhost:<your_port>/api',
};
```

3) Setup project <ECommerce/IoT>.WebApiCore as startup project

## Test User / Password

Application will use special test token for auth, but you also can use these test users for application testing:

1) login: user@user.user, password: 12345

2) login: admin@admin.admin, password: !2e4S


## Issue tracking



You can post issues and see sample materials at [GitHub Support Repository](https://github.com/akveo/ngx-admin-bundle-support/issues)