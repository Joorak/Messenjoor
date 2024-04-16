
const storage = window.localStorage;


window.getFromStorage = (key) => storage.getItem(key);

window.setToStorage = (key, value) => storage.setItem(key, value);

window.removeFromStorage = (key) => storage.removeItem(key);

window.scrollToLastMessage = () => {
    var lastMessageLi = document.querySelector('#messages-ul > li:last-child');
    if (lastMessageLi) {
        lastMessageLi.scrollIntoView({
            behavior: 'smooth'
        });
    }
};

window.startNotif = (user_id) => {

    var beamsClient = new PusherPushNotifications.Client({
        instanceId: 'f2c3249c-86c4-41e9-9fdb-d3a644514b27',
        
    });

    var pusher_token = window.localStorage.getItem("pusher_token");

    if (pusher_token != null && pusher_token.toString() != user_id) {
        beamsClient.stop()
            .then(() => console.log('Beams SDK has been stopped'))
            .catch(e => console.error('Could not stop Beams SDK', e));

        pusher_token = null;
    }

    if (pusher_token == null) {
        //Notification.requestPermission().then(permission => {
        //    if (permission === "granted") {
        //        fetch("https://f2c3249c-86c4-41e9-9fdb-d3a644514b27.pushnotifications.pusher.com/device_api/v1/instances/f2c3249c-86c4-41e9-9fdb-d3a644514b27/devices/web", {
        //            method: "POST",
        //            body: JSON.stringify({
        //                userId: 1,
        //                title: "Fix my bugs",
        //                completed: false
        //            }),
        //            headers: {
        //                "Content-type": "application/json",
        //                "Authorization": "Bearer 0F28E168CC3CDCB9E0018E0C7AF7B17307733A499B96029A8DAF7BFF863A3AF9"
        //            }
        //        })
        //            .then((response) => response.json())
        //            .then((json) => console.log(json));
        //    } else {
        //        return;
        //        console.log("Unable to get permission to notify.")
        //    }
        //});
        beamsClient.start()
            .then(() => console.log('Beams SDK has started'))
            .then(() => beamsClient.getDeviceId())

            .then(() => beamsClient.addDeviceInterest(user_id))
            .then(() => window.localStorage.setItem("pusher_token", user_id))
            //.then(() => beamsClient.setUserId(user_id, tokenProvider))
            .then(() => beamsClient.getDeviceInterests())
            .then((interests) => console.log("Current interests:", interests))

            //.then(deviceId => { console.log(deviceId); window.localStorage.setItem("pusher_token", currentToken); })
            .catch(console.error);
    }

    //if (pusher_token.toString() != user_id) {
    //    beamsClient.stop()
    //        .then(() => console.log('Beams SDK has been stopped'))
    //        .catch(e => console.error('Could not stop Beams SDK', e));
    //}

    //beamsClient
    //    .getRegistrationState()
    //    .then((state) => {
    //        let states = PusherPushNotifications.RegistrationState;
    //        switch (state) {
    //            case states.PERMISSION_DENIED: {
    //                // Notifications are blocked
    //                // Show message saying user should unblock notifications in their browser
    //                console.log("PERMISSION_DENIED");
    //                break;
    //            }
    //            case states.PERMISSION_GRANTED_REGISTERED_WITH_BEAMS: {
    //                // Ready to receive notifications
    //                // Show "Disable notifications" button, onclick calls '.stop'
    //                console.log("PERMISSION_GRANTED_REGISTERED_WITH_BEAMS");
    //                beamsClient.stop()
    //                .then(() => console.log('Beams SDK has been stopped'))
    //                    .catch(e => console.error('Could not stop Beams SDK', e));

    //                console.log("Beams SDK has stopped");

    //                beamsClient.start()
    //                    .then(() => console.log('Beams SDK has started'))
    //                    .then(() => beamsClient.getDeviceId())
                        
    //                    .then(() => beamsClient.addDeviceInterest(user_id))
    //                    .then(() => window.localStorage.setItem("pusher_token", user_id))
    //                    //.then(() => beamsClient.setUserId(user_id, tokenProvider))
    //                    .then(() => beamsClient.getDeviceInterests())
    //                    .then((interests) => console.log("Current interests:", interests))
                        
    //                    //.then(deviceId => { console.log(deviceId); window.localStorage.setItem("pusher_token", currentToken); })
    //                    .catch(console.error);

    //                break;
    //            }
    //            case states.PERMISSION_GRANTED_NOT_REGISTERED_WITH_BEAMS:
    //                console.log("PERMISSION_GRANTED_NOT_REGISTERED_WITH_BEAMS");
    //            case states.PERMISSION_PROMPT_REQUIRED: {
    //                // Need to call start before we're ready to receive notifications
    //                // Show "Enable notifications" button, onclick calls '.start'
    //                console.log("PERMISSION_PROMPT_REQUIRED");
    //                beamsClient.start()
    //                    .then(() => console.log('Beams SDK has started'))
    //                    .then(() => beamsClient.getDeviceId())

    //                    .then(() => beamsClient.addDeviceInterest(user_id))
    //                    .then(() => window.localStorage.setItem("pusher_token", user_id))
    //                    //.then(() => beamsClient.setUserId(user_id, tokenProvider))
    //                    .then(() => beamsClient.getDeviceInterests())
    //                    .then((interests) => console.log("Current interests:", interests))

    //                    //.then(deviceId => { console.log(deviceId); window.localStorage.setItem("pusher_token", currentToken); })
    //                    .catch(console.error);
    //                break;
    //            }
    //        }
    //    })
    //    .catch((e) => console.error("Could not get registration state", e));
}

