import {getEmail, logout} from './authManager.js';

document.getElementById('logout-button').addEventListener('click', logout);

const userEmail = await getEmail();
console.log(`User email: ${userEmail}`);

document.getElementById('welcome').innerText = `Welcome ${userEmail}!`;

drawGrid(50, 90);

function drawGrid(rows, cols) {
  let root = document.documentElement;
  let grid = document.getElementById('grid');
  let blockSize = 0.8 * (grid.offsetWidth / cols);
  root.style.setProperty('--block-size', `${blockSize}px`);

  for (let i = 1; i <= rows; i++) {
    let row = document.createElement('section');
    row.className = 'row';
    for (let j = 1; j <= cols; j++) {
      let block = document.createElement('div');
      block.className = 'block';
      block.dataset.row = i;
      block.dataset.col = j;
      row.appendChild(block);
    }
    grid.appendChild(row);
  }
}