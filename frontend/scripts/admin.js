// import {drawGrid} from "./index.js";

drawGrid(30, 50, false);


export function drawGrid(rows, cols, interactable = true) {
    let root = document.documentElement;
    let grid = document.getElementById('grid');
    let blockSize = 0.6 * (screen.width / cols);
    root.style.setProperty('--block-size', `${blockSize}px`);
  
    for (let i = 1; i <= rows; i++) {
      let row = document.createElement('section');
      row.className = 'row';
      for (let j = 1; j <= cols; j++) {
        let block = document.createElement('div');
        block.className = 'block';
        block.dataset.row = i;
        block.dataset.col = j;
        if (interactable) {
          block.addEventListener('click', blockClickHandler);
        }
        row.appendChild(block);
      }
      grid.appendChild(row);
    }
  }
