// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.importScripts('https://www.gstatic.com/firebasejs/10.11.0/firebase-app.js');
self.importScripts('https://www.gstatic.com/firebasejs/10.11.0/firebase-messaging.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-0007';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/];
const offlineAssetsExclude = [/^service-worker\.js$/];
// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);
const messageDivId = "messages";
const tokenDivId = "token_div";
const permissionDivId = "permission_div";

//import firebase from "firebase/compat/app";
//import { getMessaging, getToken } from "firebase/messaging";
async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

    // Also cache the host HTML and blazor.webassembly.js
    assetsRequests.push(new Request(baseUrl, { cache: 'no-cache' }));
    assetsRequests.push(new Request(new URL('_framework/blazor.web.js', baseUrl).href, { cache: 'no-cache' }));

    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));

    console.log("Requesting permission...");
    Notification.requestPermission().then(permission => {
        if (permission === "granted") {
            console.log("Notification permission granted.");
            // TODO(developer): Retrieve a registration token for use with FCM.
            // In many cases once an app has been granted notification permission,
            // it should update its UI reflecting this.

            const firebaseConfig = {
                apiKey: "AIzaSyCvOIMhNitFOFINlr2qL-hbHrcoAptoym8",
                authDomain: "messenjoor.firebaseapp.com",
                projectId: "messenjoor",
                storageBucket: "messenjoor.appspot.com",
                messagingSenderId: "827156323942",
                appId: "1:827156323942:web:db1f48c6dc6621801057a7"
            };

            // Initialize Firebase
            const app = initializeApp(firebaseConfig);


            // Initialize Firebase Cloud Messaging and get a reference to the service
            const messaging = getMessaging(app);
            // Add the public key generated from the console here.
            //getToken(messaging, { vapidKey: "AIzaSyCvOIMhNitFOFINlr2qL-hbHrcoAptoym8" });

            

            resetUI(messaging);

            messaging.onMessage(messaging, payload => {
                console.log("Message received. ", payload);
                // Update the UI to include the received message.
                appendMessage(payload);
            });

        }
        else {
            console.log("Unable to get permission to notify.")
        }
    });
}
async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}
async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve the host HTML from cache,
        // unless that request is for an offline resource.
        // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
        const shouldServeHostHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeHostHtml ? baseUrl : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    return cachedResponse || fetch(event.request);
}
function requestPermission() {
function resetUI(messaging) {
    clearMessages();
    showToken("loading...");
    // Get registration token. Initially this makes a network call, once retrieved
    // subsequent calls to getToken will return from cache.
    getToken(messaging, "AIzaSyCvOIMhNitFOFINlr2qL-hbHrcoAptoym8")
        .then(currentToken => {
            if (currentToken) {
                //sendTokenToServer(currentToken)
                window.localStorage.setItem("gcm_token", currentToken)
                updateUIForPushEnabled(currentToken);
            } else {
                // Show permission request.
                console.log(
                    "No registration token available. Request permission to generate one."
                )
                // Show permission UI.
                updateUIForPushPermissionRequired()
                setTokenSentToServer(false)
            }
        })
        .catch(err => {
            console.log("An error occurred while retrieving token. ", err)
            showToken("Error retrieving registration token.")
            setTokenSentToServer(false)
        })
}

function showToken(currentToken) {
    // Show token in console and UI.
    const tokenElement = document.querySelector("#token")
    if (tokenElement != null)
        tokenElement.textContent = currentToken
}

function showHideDiv(divId, show) {
    const div = document.querySelector("#" + divId);
    if (show) {
        div.style.display = "block";
    } else {
        div.style.display = "none";
    }
}

function appendMessage(payload) {
    const messagesElement = document.querySelector("#" + messageDivId);
    const dataHeaderElement = document.createElement("h5");
    const dataElement = document.createElement("pre");
    dataElement.style.overflowX = "hidden;";
    dataHeaderElement.textContent = "Received message:";
    dataElement.textContent = JSON.stringify(payload, null, 2);
    messagesElement.appendChild(dataHeaderElement);
    messagesElement.appendChild(dataElement);
}

// Clear the messages element of all children.
function clearMessages() {
    const messagesElement = document.querySelector("#" + messageDivId);
    while (messagesElement != null && messagesElement.hasChildNodes()) {
        messagesElement.removeChild(messagesElement.lastChild);
    };
}

function updateUIForPushEnabled(currentToken) {
    showHideDiv(tokenDivId, true);
    showHideDiv(permissionDivId, false);
    showToken(currentToken);
}

function updateUIForPushPermissionRequired() {
    showHideDiv(tokenDivId, false);
    showHideDiv(permissionDivId, true);
}
//async function onBackgroundMessage() {

//    // [START messaging_on_background_message]
//    messaging.onBackgroundMessage((payload) => {
//        console.log(
//            '[firebase-messaging-sw.js] Received background message ',
//            payload
//        );
//        // Customize notification here
//        const notificationTitle = 'Background Message Title';
//        const notificationOptions = {
//            body: 'Background Message body.',
//            icon: '/firebase-logo.png'
//        };

//        self.registration.showNotification(notificationTitle, notificationOptions);
//    });
//    // [END messaging_on_background_message]
//}

//import firebase from 'firebase/app';
//import 'firebase/messaging';

//// See: https://github.com/microsoft/TypeScript/issues/14877
///** @type {ServiceWorkerGlobalScope} */
//let self;

//function initInSw() {
//    // [START messaging_init_in_sw]
//    // Give the service worker access to Firebase Messaging.
//    // Note that you can only use Firebase Messaging here. Other Firebase libraries
//    // are not available in the service worker.
//    importScripts('https://www.gstatic.com/firebasejs/10.11.0/firebase-app.js');
//    importScripts('https://www.gstatic.com/firebasejs/10.11.0/firebase-messaging.js');

//    // Initialize the Firebase app in the service worker by passing in
//    // your app's Firebase config object.
//    // https://firebase.google.com/docs/web/setup#config-object
//    firebase.initializeApp({
//        apiKey: "AIzaSyCvOIMhNitFOFINlr2qL-hbHrcoAptoym8",
//        authDomain: "messenjoor.firebaseapp.com",
//        projectId: "messenjoor",
//        storageBucket: "messenjoor.appspot.com",
//        messagingSenderId: "827156323942",
//        appId: "1:827156323942:web:db1f48c6dc6621801057a7"
//    });

//    // Retrieve an instance of Firebase Messaging so that it can handle background
//    // messages.
//    const messaging = firebase.messaging();
//    // [END messaging_init_in_sw]
//}

//function onBackgroundMessage() {

//    // [START messaging_on_background_message]
//    messaging.onBackgroundMessage((payload) => {
//        console.log(
//            '[firebase-messaging-sw.js] Received background message ',
//            payload
//        );
//        // Customize notification here
//        const notificationTitle = 'Background Message Title';
//        const notificationOptions = {
//            body: 'Background Message body.',
//            icon: '/firebase-logo.png'
//        };

//        self.registration.showNotification(notificationTitle, notificationOptions);
//    });
//    // [END messaging_on_background_message]
//}