# ERP Lite — web client (Angular 22)

Standalone components, signals, reactive forms, lazy-loaded routes. Calls the API through relative `/api` URLs:
the dev server proxies them to `http://localhost:5080` (`proxy.conf.json`), nginx does it in Docker (`nginx.conf`).

```bash
npm install
npm start          # http://localhost:4200 (start the API first)
npx ng test        # unit tests (Vitest)
npx ng build       # production build in dist/
```

See the [root README](../README.md) for the whole project.
