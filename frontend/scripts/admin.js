drawGrid(30, 50, false);

populatePastTopics();
function populatePastTopics(){
  let topics = ['Topic 1', 'Topic 2', 'Topic 3', 'Topic 4', 'Topic 5'];
  let select = document.getElementById('topic-selector');

  topics.forEach(topic => {
    let option = document.createElement('option');
    option.text = topic;
    select.add(option);
  });
}

function drawGrid(rows, cols, interactable = true) {
    let root = document.documentElement;
    let grid = document.getElementById('grid');
    
    let gridWidth = window.innerWidth * 0.55;
    let blockSize = gridWidth / cols;
    
    root.style.setProperty('--block-size', `${blockSize}px`);
    grid.innerHTML = '';

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