const CACHE_NAME = 'lmdd-v1';
const OFFLINE_URL = '/offline.html';

const STATIC_ASSETS = [
    '/',
    '/css/marca.css',
    '/css/site.css',
    '/js/pos.js',
    '/js/cocina-kds.js',
    '/js/site.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    OFFLINE_URL
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            return cache.addAll(STATIC_ASSETS);
        }).then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames
                    .filter(name => name !== CACHE_NAME)
                    .map(name => caches.delete(name))
            );
        }).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') {
        return;
    }

    const isPageNavigation = event.request.mode === 'navigate';

    event.respondWith(
        fetch(event.request)
            .then(response => {
                if (response.ok && response.type === 'basic') {
                    const clone = response.clone();
                    caches.open(CACHE_NAME).then(cache => cache.put(event.request, clone));
                }
                return response;
            })
            .catch(() => {
                return caches.match(event.request).then(cached => {
                    if (cached) return cached;
                    if (isPageNavigation) return caches.match(OFFLINE_URL);
                    return new Response('Sin conexión', { status: 503, statusText: 'Service Unavailable' });
                });
            })
    );
});
