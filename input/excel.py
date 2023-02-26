import xlwings as xl
from collections import defaultdict

result = defaultdict(int)
result['strangeValuesPos'] = []
result['emptyCellsPos'] = []
result['outOfRangePos'] = []

xl.App(visible=False)
book = xl.Book('test_3.xls')

for sheet in book.sheets:
	print('Treat Sheet: ', sheet.name)
	ranges = sheet.range('D9:G9').expand('down')
	print('There are ', len(ranges.value), 'rows in this sheet')
	for i in range(len(ranges.value)):
		for j in range(1, len(ranges.value[i])):
			print('This value is: ', type(ranges.value[i][j]))
			if not isinstance(ranges.value[i][j], float):
				if not (ranges.value[i][j] == 'غ م' or ranges.value[i][j]=='معفى' or ranges.value[i][j]== 'معفية' or ranges.value[i][j]== None):
					result['strangeValues'] += 1
					result['strangeValuesPos'].append((sheet.name, i, j))
				elif ranges.value[i][j] == None:
					result['emptyCells'] += 1
					result['emptyCellsPos'].append((sheet.name, i, j))
				
			elif ranges.value[i][j]>20 or ranges.value[i][j]<0:
				result['outOfRange'] += 1
				result['outOfRangePos'].append((sheet.name, i, j))
	print('----------------------')

print(result)

book.close()