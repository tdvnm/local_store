module.exports=[18622,(e,t,r)=>{t.exports=e.x("next/dist/compiled/next-server/app-page-turbo.runtime.prod.js",()=>require("next/dist/compiled/next-server/app-page-turbo.runtime.prod.js"))},56704,(e,t,r)=>{t.exports=e.x("next/dist/server/app-render/work-async-storage.external.js",()=>require("next/dist/server/app-render/work-async-storage.external.js"))},32319,(e,t,r)=>{t.exports=e.x("next/dist/server/app-render/work-unit-async-storage.external.js",()=>require("next/dist/server/app-render/work-unit-async-storage.external.js"))},24725,(e,t,r)=>{t.exports=e.x("next/dist/server/app-render/after-task-async-storage.external.js",()=>require("next/dist/server/app-render/after-task-async-storage.external.js"))},70406,(e,t,r)=>{t.exports=e.x("next/dist/compiled/@opentelemetry/api",()=>require("next/dist/compiled/@opentelemetry/api"))},14747,(e,t,r)=>{t.exports=e.x("path",()=>require("path"))},93695,(e,t,r)=>{t.exports=e.x("next/dist/shared/lib/no-fallback-error.external.js",()=>require("next/dist/shared/lib/no-fallback-error.external.js"))},85148,(e,t,r)=>{t.exports=e.x("better-sqlite3-90e2652d1716b047",()=>require("better-sqlite3-90e2652d1716b047"))},43793,e=>{"use strict";var t=e.i(85148);let r=e.i(14747).default.join(process.cwd(),"lucky_store.db"),a=new t.default(r);a.pragma("journal_mode = WAL"),a.pragma("foreign_keys = ON"),a.exec(`
  CREATE TABLE IF NOT EXISTS products (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    price REAL NOT NULL,
    category TEXT NOT NULL,
    unit TEXT NOT NULL DEFAULT 'piece',
    in_stock INTEGER NOT NULL DEFAULT 1,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
  );

  CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    flat_no TEXT NOT NULL UNIQUE,
    phone TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
  );

  CREATE TABLE IF NOT EXISTS orders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    status TEXT NOT NULL DEFAULT 'pending',
    payment_method TEXT NOT NULL DEFAULT 'cod',
    total REAL NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (user_id) REFERENCES users(id)
  );

  CREATE TABLE IF NOT EXISTS order_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1,
    price REAL NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
  );

  CREATE TABLE IF NOT EXISTS tab_payments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    amount REAL NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (user_id) REFERENCES users(id)
  );
`),e.s(["default",0,a])},33489,e=>{"use strict";var t=e.i(47909),r=e.i(74017),a=e.i(96250),n=e.i(59756),s=e.i(61916),i=e.i(74677),o=e.i(69741),d=e.i(16795),l=e.i(87718),u=e.i(95169),p=e.i(47587),E=e.i(66012),c=e.i(70101),T=e.i(26937),N=e.i(10372),R=e.i(93695);e.i(52474);var L=e.i(220),h=e.i(89171),x=e.i(43793);async function A(){let e=x.default.prepare(`
    SELECT
      u.id as user_id,
      u.name,
      u.flat_no,
      u.phone,
      COALESCE(SUM(CASE WHEN o.payment_method = 'tab' AND o.status != 'cancelled' THEN o.total ELSE 0 END), 0) as total_tab,
      COALESCE((SELECT SUM(tp.amount) FROM tab_payments tp WHERE tp.user_id = u.id), 0) as total_paid
    FROM users u
    LEFT JOIN orders o ON o.user_id = u.id
    GROUP BY u.id
    HAVING total_tab > 0 OR total_paid > 0
    ORDER BY u.flat_no
  `).all().map(e=>({...e,balance:e.total_tab-e.total_paid}));return h.NextResponse.json(e)}async function O(e){let{user_id:t,amount:r}=await e.json();return x.default.prepare("INSERT INTO tab_payments (user_id, amount) VALUES (?, ?)").run(t,r),h.NextResponse.json({ok:!0},{status:201})}e.s(["GET",0,A,"POST",0,O],20976);var m=e.i(20976);let v=new t.AppRouteRouteModule({definition:{kind:r.RouteKind.APP_ROUTE,page:"/api/tabs/route",pathname:"/api/tabs",filename:"route",bundlePath:""},distDir:".next",relativeProjectDir:"",resolvedPagePath:"[project]/src/app/api/tabs/route.ts",nextConfigOutput:"",userland:m,...{}}),{workAsyncStorage:U,workUnitAsyncStorage:C,serverHooks:_}=v;async function I(e,t,a){a.requestMeta&&(0,n.setRequestMeta)(e,a.requestMeta),v.isDev&&(0,n.addRequestMeta)(e,"devRequestTimingInternalsEnd",process.hrtime.bigint());let h="/api/tabs/route";h=h.replace(/\/index$/,"")||"/";let x=await v.prepare(e,t,{srcPage:h,multiZoneDraftMode:!1});if(!x)return t.statusCode=400,t.end("Bad Request"),null==a.waitUntil||a.waitUntil.call(a,Promise.resolve()),null;let{buildId:A,params:O,nextConfig:m,parsedUrl:U,isDraftMode:C,prerenderManifest:_,routerServerContext:I,isOnDemandRevalidate:f,revalidateOnlyGenerated:g,resolvedPathname:w,clientReferenceManifest:S,serverActionsManifest:b}=x,y=(0,o.normalizeAppPath)(h),F=!!(_.dynamicRoutes[y]||_.routes[w]),P=async()=>((null==I?void 0:I.render404)?await I.render404(e,t,U,!1):t.end("This page could not be found"),null);if(F&&!C){let e=!!_.routes[w],t=_.dynamicRoutes[y];if(t&&!1===t.fallback&&!e){if(m.adapterPath)return await P();throw new R.NoFallbackError}}let q=null;!F||v.isDev||C||(q="/index"===(q=w)?"/":q);let M=!0===v.isDev||!F,D=F&&!M;b&&S&&(0,i.setManifestsSingleton)({page:h,clientReferenceManifest:S,serverActionsManifest:b});let k=e.method||"GET",j=(0,s.getTracer)(),G=j.getActiveScopeSpan(),H=!!(null==I?void 0:I.isWrappedByNextServer),X=!!(0,n.getRequestMeta)(e,"minimalMode"),K=(0,n.getRequestMeta)(e,"incrementalCache")||await v.getIncrementalCache(e,m,_,X);null==K||K.resetRequestCache(),globalThis.__incrementalCache=K;let Y={params:O,previewProps:_.preview,renderOpts:{experimental:{authInterrupts:!!m.experimental.authInterrupts},cacheComponents:!!m.cacheComponents,supportsDynamicResponse:M,incrementalCache:K,cacheLifeProfiles:m.cacheLife,waitUntil:a.waitUntil,onClose:e=>{t.on("close",e)},onAfterTaskError:void 0,onInstrumentationRequestError:(t,r,a,n)=>v.onRequestError(e,t,a,n,I)},sharedContext:{buildId:A}},B=new d.NodeNextRequest(e),$=new d.NodeNextResponse(t),W=l.NextRequestAdapter.fromNodeNextRequest(B,(0,l.signalFromNodeResponse)(t));try{let n,i=async e=>v.handle(W,Y).finally(()=>{if(!e)return;e.setAttributes({"http.status_code":t.statusCode,"next.rsc":!1});let r=j.getRootSpanAttributes();if(!r)return;if(r.get("next.span_type")!==u.BaseServerSpan.handleRequest)return void console.warn(`Unexpected root span type '${r.get("next.span_type")}'. Please report this Next.js issue https://github.com/vercel/next.js`);let a=r.get("next.route");if(a){let t=`${k} ${a}`;e.setAttributes({"next.route":a,"http.route":a,"next.span_name":t}),e.updateName(t),n&&n!==e&&(n.setAttribute("http.route",a),n.updateName(t))}else e.updateName(`${k} ${h}`)}),o=async n=>{var s,o;let d=async({previousCacheEntry:r})=>{try{if(!X&&f&&g&&!r)return t.statusCode=404,t.setHeader("x-nextjs-cache","REVALIDATED"),t.end("This page could not be found"),null;let s=await i(n);e.fetchMetrics=Y.renderOpts.fetchMetrics;let o=Y.renderOpts.pendingWaitUntil;o&&a.waitUntil&&(a.waitUntil(o),o=void 0);let d=Y.renderOpts.collectedTags;if(!F)return await (0,E.sendResponse)(B,$,s,Y.renderOpts.pendingWaitUntil),null;{let e=await s.blob(),t=(0,c.toNodeOutgoingHttpHeaders)(s.headers);d&&(t[N.NEXT_CACHE_TAGS_HEADER]=d),!t["content-type"]&&e.type&&(t["content-type"]=e.type);let r=void 0!==Y.renderOpts.collectedRevalidate&&!(Y.renderOpts.collectedRevalidate>=N.INFINITE_CACHE)&&Y.renderOpts.collectedRevalidate,a=void 0===Y.renderOpts.collectedExpire||Y.renderOpts.collectedExpire>=N.INFINITE_CACHE?void 0:Y.renderOpts.collectedExpire;return{value:{kind:L.CachedRouteKind.APP_ROUTE,status:s.status,body:Buffer.from(await e.arrayBuffer()),headers:t},cacheControl:{revalidate:r,expire:a}}}}catch(t){throw(null==r?void 0:r.isStale)&&await v.onRequestError(e,t,{routerKind:"App Router",routePath:h,routeType:"route",revalidateReason:(0,p.getRevalidateReason)({isStaticGeneration:D,isOnDemandRevalidate:f})},!1,I),t}},l=await v.handleResponse({req:e,nextConfig:m,cacheKey:q,routeKind:r.RouteKind.APP_ROUTE,isFallback:!1,prerenderManifest:_,isRoutePPREnabled:!1,isOnDemandRevalidate:f,revalidateOnlyGenerated:g,responseGenerator:d,waitUntil:a.waitUntil,isMinimalMode:X});if(!F)return null;if((null==l||null==(s=l.value)?void 0:s.kind)!==L.CachedRouteKind.APP_ROUTE)throw Object.defineProperty(Error(`Invariant: app-route received invalid cache entry ${null==l||null==(o=l.value)?void 0:o.kind}`),"__NEXT_ERROR_CODE",{value:"E701",enumerable:!1,configurable:!0});X||t.setHeader("x-nextjs-cache",f?"REVALIDATED":l.isMiss?"MISS":l.isStale?"STALE":"HIT"),C&&t.setHeader("Cache-Control","private, no-cache, no-store, max-age=0, must-revalidate");let u=(0,c.fromNodeOutgoingHttpHeaders)(l.value.headers);return X&&F||u.delete(N.NEXT_CACHE_TAGS_HEADER),!l.cacheControl||t.getHeader("Cache-Control")||u.get("Cache-Control")||u.set("Cache-Control",(0,T.getCacheControlHeader)(l.cacheControl)),await (0,E.sendResponse)(B,$,new Response(l.value.body,{headers:u,status:l.value.status||200})),null};H&&G?await o(G):(n=j.getActiveScopeSpan(),await j.withPropagatedContext(e.headers,()=>j.trace(u.BaseServerSpan.handleRequest,{spanName:`${k} ${h}`,kind:s.SpanKind.SERVER,attributes:{"http.method":k,"http.target":e.url}},o),void 0,!H))}catch(t){if(t instanceof R.NoFallbackError||await v.onRequestError(e,t,{routerKind:"App Router",routePath:y,routeType:"route",revalidateReason:(0,p.getRevalidateReason)({isStaticGeneration:D,isOnDemandRevalidate:f})},!1,I),F)throw t;return await (0,E.sendResponse)(B,$,new Response(null,{status:500})),null}}e.s(["handler",0,I,"patchFetch",0,function(){return(0,a.patchFetch)({workAsyncStorage:U,workUnitAsyncStorage:C})},"routeModule",0,v,"serverHooks",0,_,"workAsyncStorage",0,U,"workUnitAsyncStorage",0,C],33489)}];

//# sourceMappingURL=%5Broot-of-the-server%5D__10sge~u._.js.map