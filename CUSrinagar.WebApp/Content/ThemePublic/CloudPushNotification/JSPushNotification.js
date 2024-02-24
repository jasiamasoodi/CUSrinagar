$(document).ready(function ($) {
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


    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/firebase-messaging-sw.js')
            .then(function (registration) {
                messaging.useServiceWorker(registration);
            }).catch(function (error) {
                console.log('Service Worker Error', err);
            });
    }

    const messaging = firebase.messaging();
    var permission;
    if ("Notification" in window) {
        permission = Notification.permission;
    }

    messaging.requestPermission()
        .then(function () {
            if (permission === "default") {
                return messaging.getToken();
            } else {
                return "";
            }
        }).then(function (token) {
            if (token !== "") {
                SaveToken(token);
            }
        })
        .catch(function (err) {

        });

    messaging.onMessage(function (payload) {
        var pushNofitication = new Notification(payload.notification.title, {
            body: payload.notification.body,
            icon: payload.notification.icon
        });
        pushNofitication.onclick = function () {
            window.open(payload.notification.click_action);
        };
    });

    function SaveToken(token) {
        var deviceInfo = navigator.userAgent;
        $.ajax({
            url: "/Home/SaveTokenAsync",
            type: "POST",
            data: { token: token, DeviceInfo: deviceInfo },
            dataType: "json",
            async: true,
            success: function (response) {
            },
            error: function (jqXHR, textStatus, errorThrown) {
            }
        });
    }
});
