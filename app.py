import eel
from pathlib import Path

inputPath = Path('./') / 'input'

eel.init('www')

@eel.expose
def getFiles():
    global inputPath
    
    return [file.name for file in inputPath.glob('*.xls')]

eel.start('index.html')