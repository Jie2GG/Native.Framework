importScripts(
  'https://storage.googleapis.com/workbox-cdn/releases/3.6.1/workbox-sw.js'
)

const ALLOWED_HOSTS = [
  location.host,
  'unpkg.com'
]

const matchCb = ({ url, event }) => {
  return event.request.method === 'GET' && ALLOWED_HOSTS.includes(url.host)
}

workbox.routing.registerRoute(
  matchCb,
  workbox.strategies.networkFirst()
)

//self.addEventListener('install', e => {
//  self.skipWaiting()
//})

//self.addEventListener('activate', e => {
//  self.registration
//    .unregister()
//    .then(() => {
//      return self.clients.matchAll()
//    })
//    .then(clients => {
//      clients.forEach(client => client.navigate(client.url))
//    })
//})