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
`),e.s(["default",0,a])},36883,e=>{"use strict";var t=e.i(47909),r=e.i(74017),a=e.i(96250),n=e.i(59756),s=e.i(61916),i=e.i(74677),o=e.i(69741),d=e.i(16795),u=e.i(87718),l=e.i(95169),p=e.i(47587),E=e.i(66012),c=e.i(70101),T=e.i(26937),R=e.i(10372),N=e.i(93695);e.i(52474);var h=e.i(220),L=e.i(89171),x=e.i(43793);async function A(e){let{searchParams:t}=new URL(e.url),r=t.get("user_id"),a=t.get("status"),n=`
    SELECT o.*, u.name as user_name, u.flat_no
    FROM orders o
    JOIN users u ON o.user_id = u.id
  `,s=[],i=[];r&&(s.push("o.user_id = ?"),i.push(Number(r))),a&&(s.push("o.status = ?"),i.push(a)),s.length>0&&(n+=" WHERE "+s.join(" AND ")),n+=" ORDER BY o.created_at DESC";let o=x.default.prepare(n).all(...i),d=x.default.prepare(`
    SELECT oi.*, p.name as product_name, p.category
    FROM order_items oi
    JOIN products p ON oi.product_id = p.id
    WHERE oi.order_id = ?
  `);for(let e of o)e.items=d.all(e.id);return L.NextResponse.json(o)}async function O(e){let{user_id:t,payment_method:r,items:a}=await e.json(),n=0,s=[];for(let e of a){let t=x.default.prepare("SELECT * FROM products WHERE id = ?").get(e.product_id);t&&(n+=t.price*e.quantity,s.push({product_id:e.product_id,quantity:e.quantity,price:t.price}))}let i=x.default.prepare("INSERT INTO orders (user_id, payment_method, total, status) VALUES (?, ?, ?, 'pending')").run(t,r||"cod",n).lastInsertRowid,o=x.default.prepare("INSERT INTO order_items (order_id, product_id, quantity, price) VALUES (?, ?, ?, ?)");return x.default.transaction(()=>{for(let e of s)o.run(i,e.product_id,e.quantity,e.price)})(),L.NextResponse.json({id:i,total:n,status:"pending"},{status:201})}e.s(["GET",0,A,"POST",0,O],38876);var m=e.i(38876);let f=new t.AppRouteRouteModule({definition:{kind:r.RouteKind.APP_ROUTE,page:"/api/orders/route",pathname:"/api/orders",filename:"route",bundlePath:""},distDir:".next",relativeProjectDir:"",resolvedPagePath:"[project]/src/app/api/orders/route.ts",nextConfigOutput:"",userland:m,...{}}),{workAsyncStorage:_,workUnitAsyncStorage:g,serverHooks:v}=f;async function I(e,t,a){a.requestMeta&&(0,n.setRequestMeta)(e,a.requestMeta),f.isDev&&(0,n.addRequestMeta)(e,"devRequestTimingInternalsEnd",process.hrtime.bigint());let L="/api/orders/route";L=L.replace(/\/index$/,"")||"/";let x=await f.prepare(e,t,{srcPage:L,multiZoneDraftMode:!1});if(!x)return t.statusCode=400,t.end("Bad Request"),null==a.waitUntil||a.waitUntil.call(a,Promise.resolve()),null;let{buildId:A,params:O,nextConfig:m,parsedUrl:_,isDraftMode:g,prerenderManifest:v,routerServerContext:I,isOnDemandRevalidate:U,revalidateOnlyGenerated:C,resolvedPathname:w,clientReferenceManifest:y,serverActionsManifest:S}=x,b=(0,o.normalizeAppPath)(L),q=!!(v.dynamicRoutes[b]||v.routes[w]),F=async()=>((null==I?void 0:I.render404)?await I.render404(e,t,_,!1):t.end("This page could not be found"),null);if(q&&!g){let e=!!v.routes[w],t=v.dynamicRoutes[b];if(t&&!1===t.fallback&&!e){if(m.adapterPath)return await F();throw new N.NoFallbackError}}let P=null;!q||f.isDev||g||(P="/index"===(P=w)?"/":P);let M=!0===f.isDev||!q,D=q&&!M;S&&y&&(0,i.setManifestsSingleton)({page:L,clientReferenceManifest:y,serverActionsManifest:S});let k=e.method||"GET",j=(0,s.getTracer)(),H=j.getActiveScopeSpan(),X=!!(null==I?void 0:I.isWrappedByNextServer),G=!!(0,n.getRequestMeta)(e,"minimalMode"),K=(0,n.getRequestMeta)(e,"incrementalCache")||await f.getIncrementalCache(e,m,v,G);null==K||K.resetRequestCache(),globalThis.__incrementalCache=K;let Y={params:O,previewProps:v.preview,renderOpts:{experimental:{authInterrupts:!!m.experimental.authInterrupts},cacheComponents:!!m.cacheComponents,supportsDynamicResponse:M,incrementalCache:K,cacheLifeProfiles:m.cacheLife,waitUntil:a.waitUntil,onClose:e=>{t.on("close",e)},onAfterTaskError:void 0,onInstrumentationRequestError:(t,r,a,n)=>f.onRequestError(e,t,a,n,I)},sharedContext:{buildId:A}},B=new d.NodeNextRequest(e),$=new d.NodeNextResponse(t),W=u.NextRequestAdapter.fromNodeNextRequest(B,(0,u.signalFromNodeResponse)(t));try{let n,i=async e=>f.handle(W,Y).finally(()=>{if(!e)return;e.setAttributes({"http.status_code":t.statusCode,"next.rsc":!1});let r=j.getRootSpanAttributes();if(!r)return;if(r.get("next.span_type")!==l.BaseServerSpan.handleRequest)return void console.warn(`Unexpected root span type '${r.get("next.span_type")}'. Please report this Next.js issue https://github.com/vercel/next.js`);let a=r.get("next.route");if(a){let t=`${k} ${a}`;e.setAttributes({"next.route":a,"http.route":a,"next.span_name":t}),e.updateName(t),n&&n!==e&&(n.setAttribute("http.route",a),n.updateName(t))}else e.updateName(`${k} ${L}`)}),o=async n=>{var s,o;let d=async({previousCacheEntry:r})=>{try{if(!G&&U&&C&&!r)return t.statusCode=404,t.setHeader("x-nextjs-cache","REVALIDATED"),t.end("This page could not be found"),null;let s=await i(n);e.fetchMetrics=Y.renderOpts.fetchMetrics;let o=Y.renderOpts.pendingWaitUntil;o&&a.waitUntil&&(a.waitUntil(o),o=void 0);let d=Y.renderOpts.collectedTags;if(!q)return await (0,E.sendResponse)(B,$,s,Y.renderOpts.pendingWaitUntil),null;{let e=await s.blob(),t=(0,c.toNodeOutgoingHttpHeaders)(s.headers);d&&(t[R.NEXT_CACHE_TAGS_HEADER]=d),!t["content-type"]&&e.type&&(t["content-type"]=e.type);let r=void 0!==Y.renderOpts.collectedRevalidate&&!(Y.renderOpts.collectedRevalidate>=R.INFINITE_CACHE)&&Y.renderOpts.collectedRevalidate,a=void 0===Y.renderOpts.collectedExpire||Y.renderOpts.collectedExpire>=R.INFINITE_CACHE?void 0:Y.renderOpts.collectedExpire;return{value:{kind:h.CachedRouteKind.APP_ROUTE,status:s.status,body:Buffer.from(await e.arrayBuffer()),headers:t},cacheControl:{revalidate:r,expire:a}}}}catch(t){throw(null==r?void 0:r.isStale)&&await f.onRequestError(e,t,{routerKind:"App Router",routePath:L,routeType:"route",revalidateReason:(0,p.getRevalidateReason)({isStaticGeneration:D,isOnDemandRevalidate:U})},!1,I),t}},u=await f.handleResponse({req:e,nextConfig:m,cacheKey:P,routeKind:r.RouteKind.APP_ROUTE,isFallback:!1,prerenderManifest:v,isRoutePPREnabled:!1,isOnDemandRevalidate:U,revalidateOnlyGenerated:C,responseGenerator:d,waitUntil:a.waitUntil,isMinimalMode:G});if(!q)return null;if((null==u||null==(s=u.value)?void 0:s.kind)!==h.CachedRouteKind.APP_ROUTE)throw Object.defineProperty(Error(`Invariant: app-route received invalid cache entry ${null==u||null==(o=u.value)?void 0:o.kind}`),"__NEXT_ERROR_CODE",{value:"E701",enumerable:!1,configurable:!0});G||t.setHeader("x-nextjs-cache",U?"REVALIDATED":u.isMiss?"MISS":u.isStale?"STALE":"HIT"),g&&t.setHeader("Cache-Control","private, no-cache, no-store, max-age=0, must-revalidate");let l=(0,c.fromNodeOutgoingHttpHeaders)(u.value.headers);return G&&q||l.delete(R.NEXT_CACHE_TAGS_HEADER),!u.cacheControl||t.getHeader("Cache-Control")||l.get("Cache-Control")||l.set("Cache-Control",(0,T.getCacheControlHeader)(u.cacheControl)),await (0,E.sendResponse)(B,$,new Response(u.value.body,{headers:l,status:u.value.status||200})),null};X&&H?await o(H):(n=j.getActiveScopeSpan(),await j.withPropagatedContext(e.headers,()=>j.trace(l.BaseServerSpan.handleRequest,{spanName:`${k} ${L}`,kind:s.SpanKind.SERVER,attributes:{"http.method":k,"http.target":e.url}},o),void 0,!X))}catch(t){if(t instanceof N.NoFallbackError||await f.onRequestError(e,t,{routerKind:"App Router",routePath:b,routeType:"route",revalidateReason:(0,p.getRevalidateReason)({isStaticGeneration:D,isOnDemandRevalidate:U})},!1,I),q)throw t;return await (0,E.sendResponse)(B,$,new Response(null,{status:500})),null}}e.s(["handler",0,I,"patchFetch",0,function(){return(0,a.patchFetch)({workAsyncStorage:_,workUnitAsyncStorage:g})},"routeModule",0,f,"serverHooks",0,v,"workAsyncStorage",0,_,"workUnitAsyncStorage",0,g],36883)}];

//# sourceMappingURL=%5Broot-of-the-server%5D__0oo3lt7._.js.map