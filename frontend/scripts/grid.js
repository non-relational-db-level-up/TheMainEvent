let root = document.documentElement;
let grid = document.getElementById('grid');

function clearGrid(rows, cols) {
  for (let i = 1; i <= rows; i++) {
    for (let j = 1; j <= cols; j++) {
      let block = document.getElementById(`block-${i}-${j}`);
      block.style.backgroundColor = 'white';
    }
  }
}

function drawGrid(rows, cols, gridRatio = 0.6,  onClickFunction) {
  let blockWidth = gridRatio * (screen.width / cols);
  let blockHeight = gridRatio * (screen.height / rows);
  let blockSize = Math.min(blockWidth, blockHeight);
  root.style.setProperty('--block-size', `${blockSize}px`);

  for (let i = 1; i <= rows; i++) {
    let row = document.createElement('section');
    row.className = 'row';
    for (let j = 1; j <= cols; j++) {
      let block = document.createElement('div');
      block.className = 'block';
      block.id = `block-${i}-${j}`;
      block.dataset.row = i;
      block.dataset.col = j;
      if (onClickFunction) {
        block.addEventListener('click', onClickFunction);
      }
      row.appendChild(block);
    }
    grid.appendChild(row);
  }
}

export {clearGrid, drawGrid }