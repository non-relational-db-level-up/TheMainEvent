function parseSecondsToTimeLeft(seconds) {
  if (seconds < 0 || isNaN(seconds)) {
    return '00:00';
  }
  let minutes = Math.floor(seconds / 60);
  let remainingSeconds = seconds % 60;
  return `${minutes < 10 ? '0' : ''}${minutes}:${remainingSeconds < 10 ? '0' : ''}${remainingSeconds}`;
}

export { parseSecondsToTimeLeft };