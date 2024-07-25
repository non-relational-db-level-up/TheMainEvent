import { backendUrl } from "./apiConfig.js";

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${backendUrl}/chatHub`, {
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

export { connection, start };