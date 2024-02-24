importScripts('https://www.gstatic.com/firebasejs/5.9.1/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/5.9.1/firebase-messaging.js');

// Initialize Firebase
var config = {
    apiKey: "AAAA6_5s1qI:APA91bHl4VVbcYm043KWLC8sktpdKMHEsUaJhAk4jxIH01EXxkcj1TMadkpOzujlVIKntsa9jt9cUWaWzScdV7oNceVtHer2cEVmuSL2Y-zkfdhOQAiHWqKlhT_L_-4AAHc08mbiBSie",
    authDomain: "cusrinagarwebapp.firebaseapp.com",
    databaseURL: "https://cusrinagarwebapp.firebaseio.com",
    projectId: "cusrinagarwebapp",
    storageBucket: "cusrinagarwebapp.appspot.com",
    messagingSenderId: "1013585860258"
};
firebase.initializeApp(config);

const messaging = firebase.messaging();

messaging.setBackgroundMessageHandler(function (payload) {
    var notificationTitle = payload.notification.title;
    var notificationOptions = {
        body: payload.notification.body,
        icon: payload.notification.icon
    };

    return self.registration.showNotification(notificationTitle, notificationOptions);
});