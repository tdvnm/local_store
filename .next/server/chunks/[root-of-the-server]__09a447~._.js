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
`),e.s(["default",0,a])},49295,e=>{"use strict";var t=e.i(47909),r=e.i(74017),a=e.i(96250),n=e.i(59756),s=e.i(61916),i=e.i(74677),o=e.i(69741),d=e.i(16795),l=e.i(87718),u=e.i(95169),p=e.i(47587),E=e.i(66012),c=e.i(70101),T=e.i(26937),R=e.i(10372),N=e.i(93695);e.i(52474);var h=e.i(220),x=e.i(89171),L=e.i(43793);async function A(e,{params:t}){let{id:r}=await t,{status:a}=await e.json();if(!["pending","confirmed","ready","delivered","cancelled"].includes(a))return x.NextResponse.json({error:"Invalid status"},{status:400});L.default.prepare("UPDATE orders SET status = ? WHERE id = ?").run(a,Number(r));let n=L.default.prepare("SELECT * FROM orders WHERE id = ?").get(Number(r));return x.NextResponse.json(n)}e.s(["PATCH",0,A],50912);var v=e.i(50912);let m=new t.AppRouteRouteModule({definition:{kind:r.RouteKind.APP_ROUTE,page:"/api/orders/[id]/route",pathname:"/api/orders/[id]",filename:"route",bundlePath:""},distDir:".next",relativeProjectDir:"",resolvedPagePath:"[project]/src/app/api/orders/[id]/route.ts",nextConfigOutput:"",userland:v,...{}}),{workAsyncStorage:O,workUnitAsyncStorage:U,serverHooks:g}=m;async function f(e,t,a){a.requestMeta&&(0,n.setRequestMeta)(e,a.requestMeta),m.isDev&&(0,n.addRequestMeta)(e,"devRequestTimingInternalsEnd",process.hrtime.bigint());let x="/api/orders/[id]/route";x=x.replace(/\/index$/,"")||"/";let L=await m.prepare(e,t,{srcPage:x,multiZoneDraftMode:!1});if(!L)return t.statusCode=400,t.end("Bad Request"),null==a.waitUntil||a.waitUntil.call(a,Promise.resolve()),null;let{buildId:A,params:v,nextConfig:O,parsedUrl:U,isDraftMode:g,prerenderManifest:f,routerServerContext:C,isOnDemandRevalidate:I,revalidateOnlyGenerated:w,resolvedPathname:y,clientReferenceManifest:_,serverActionsManifest:S}=L,b=(0,o.normalizeAppPath)(x),F=!!(f.dynamicRoutes[b]||f.routes[y]),P=async()=>((null==C?void 0:C.render404)?await C.render404(e,t,U,!1):t.end("This page could not be found"),null);if(F&&!g){let e=!!f.routes[y],t=f.dynamicRoutes[b];if(t&&!1===t.fallback&&!e){if(O.adapterPath)return await P();throw new N.NoFallbackError}}let q=null;!F||m.isDev||g||(q="/index"===(q=y)?"/":q);let M=!0===m.isDev||!F,k=F&&!M;S&&_&&(0,i.setManifestsSingleton)({page:x,clientReferenceManifest:_,serverActionsManifest:S});let D=e.method||"GET",j=(0,s.getTracer)(),H=j.getActiveScopeSpan(),X=!!(null==C?void 0:C.isWrappedByNextServer),G=!!(0,n.getRequestMeta)(e,"minimalMode"),K=(0,n.getRequestMeta)(e,"incrementalCache")||await m.getIncrementalCache(e,O,f,G);null==K||K.resetRequestCache(),globalThis.__incrementalCache=K;let Y={params:v,previewProps:f.preview,renderOpts:{experimental:{authInterrupts:!!O.experimental.authInterrupts},cacheComponents:!!O.cacheComponents,supportsDynamicResponse:M,incrementalCache:K,cacheLifeProfiles:O.cacheLife,waitUntil:a.waitUntil,onClose:e=>{t.on("close",e)},onAfterTaskError:void 0,onInstrumentationRequestError:(t,r,a,n)=>m.onRequestError(e,t,a,n,C)},sharedContext:{buildId:A}},B=new d.NodeNextRequest(e),$=new d.NodeNextResponse(t),W=l.NextRequestAdapter.fromNodeNextRequest(B,(0,l.signalFromNodeResponse)(t));try{let n,i=async e=>m.handle(W,Y).finally(()=>{if(!e)return;e.setAttributes({"http.status_code":t.statusCode,"next.rsc":!1});let r=j.getRootSpanAttributes();if(!r)return;if(r.get("next.span_type")!==u.BaseServerSpan.handleRequest)return void console.warn(`Unexpected root span type '${r.get("next.span_type")}'. Please report this Next.js issue https://github.com/vercel/next.js`);let a=r.get("next.route");if(a){let t=`${D} ${a}`;e.setAttributes({"next.route":a,"http.route":a,"next.span_name":t}),e.updateName(t),n&&n!==e&&(n.setAttribute("http.route",a),n.updateName(t))}else e.updateName(`${D} ${x}`)}),o=async n=>{var s,o;let d=async({previousCacheEntry:r})=>{try{if(!G&&I&&w&&!r)return t.statusCode=404,t.setHeader("x-nextjs-cache","REVALIDATED"),t.end("This page could not be found"),null;let s=await i(n);e.fetchMetrics=Y.renderOpts.fetchMetrics;let o=Y.renderOpts.pendingWaitUntil;o&&a.waitUntil&&(a.waitUntil(o),o=void 0);let d=Y.renderOpts.collectedTags;if(!F)return await (0,E.sendResponse)(B,$,s,Y.renderOpts.pendingWaitUntil),null;{let e=await s.blob(),t=(0,c.toNodeOutgoingHttpHeaders)(s.headers);d&&(t[R.NEXT_CACHE_TAGS_HEADER]=d),!t["content-type"]&&e.type&&(t["content-type"]=e.type);let r=void 0!==Y.renderOpts.collectedRevalidate&&!(Y.renderOpts.collectedRevalidate>=R.INFINITE_CACHE)&&Y.renderOpts.collectedRevalidate,a=void 0===Y.renderOpts.collectedExpire||Y.renderOpts.collectedExpire>=R.INFINITE_CACHE?void 0:Y.renderOpts.collectedExpire;return{value:{kind:h.CachedRouteKind.APP_ROUTE,status:s.status,body:Buffer.from(await e.arrayBuffer()),headers:t},cacheControl:{revalidate:r,expire:a}}}}catch(t){throw(null==r?void 0:r.isStale)&&await m.onRequestError(e,t,{routerKind:"App Router",routePath:x,routeType:"route",revalidateReason:(0,p.getRevalidateReason)({isStaticGeneration:k,isOnDemandRevalidate:I})},!1,C),t}},l=await m.handleResponse({req:e,nextConfig:O,cacheKey:q,routeKind:r.RouteKind.APP_ROUTE,isFallback:!1,prerenderManifest:f,isRoutePPREnabled:!1,isOnDemandRevalidate:I,revalidateOnlyGenerated:w,responseGenerator:d,waitUntil:a.waitUntil,isMinimalMode:G});if(!F)return null;if((null==l||null==(s=l.value)?void 0:s.kind)!==h.CachedRouteKind.APP_ROUTE)throw Object.defineProperty(Error(`Invariant: app-route received invalid cache entry ${null==l||null==(o=l.value)?void 0:o.kind}`),"__NEXT_ERROR_CODE",{value:"E701",enumerable:!1,configurable:!0});G||t.setHeader("x-nextjs-cache",I?"REVALIDATED":l.isMiss?"MISS":l.isStale?"STALE":"HIT"),g&&t.setHeader("Cache-Control","private, no-cache, no-store, max-age=0, must-revalidate");let u=(0,c.fromNodeOutgoingHttpHeaders)(l.value.headers);return G&&F||u.delete(R.NEXT_CACHE_TAGS_HEADER),!l.cacheControl||t.getHeader("Cache-Control")||u.get("Cache-Control")||u.set("Cache-Control",(0,T.getCacheControlHeader)(l.cacheControl)),await (0,E.sendResponse)(B,$,new Response(l.value.body,{headers:u,status:l.value.status||200})),null};X&&H?await o(H):(n=j.getActiveScopeSpan(),await j.withPropagatedContext(e.headers,()=>j.trace(u.BaseServerSpan.handleRequest,{spanName:`${D} ${x}`,kind:s.SpanKind.SERVER,attributes:{"http.method":D,"http.target":e.url}},o),void 0,!X))}catch(t){if(t instanceof N.NoFallbackError||await m.onRequestError(e,t,{routerKind:"App Router",routePath:b,routeType:"route",revalidateReason:(0,p.getRevalidateReason)({isStaticGeneration:k,isOnDemandRevalidate:I})},!1,C),F)throw t;return await (0,E.sendResponse)(B,$,new Response(null,{status:500})),null}}e.s(["handler",0,f,"patchFetch",0,function(){return(0,a.patchFetch)({workAsyncStorage:O,workUnitAsyncStorage:U})},"routeModule",0,m,"serverHooks",0,g,"workAsyncStorage",0,O,"workUnitAsyncStorage",0,U],49295)}];

//# sourceMappingURL=%5Broot-of-the-server%5D__09a447~._.js.map