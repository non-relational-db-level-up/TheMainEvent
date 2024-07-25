import { backendUrl } from "./apiConfig";
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/chatHub", {
        accessTokenFactory: () => sessionStorage.getItem('accessToken')
    })
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});

connection.on("ReceiveMessage", (user, message) => {
    console.log(`${user}: ${message}`);
});

export { connection, start };