function parseSecondsToTimeLeft(seconds) {
  let minutes = Math.floor(seconds / 60);
  let remainingSeconds = seconds % 60;
  return `${minutes < 10 ? '0' : ''}${minutes}:${remainingSeconds < 10 ? '0' : ''}${remainingSeconds}`;
}

function calculateSecondsLeftFromDateTIme(time) {
  return Math.floor((new Date(time) - new Date()) / 1000);
}

export { parseSecondsToTimeLeft, calculateSecondsLeftFromDateTIme };