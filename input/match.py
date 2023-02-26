import sys
#The sys library is for using it later to catch the command line args

def correct(value):
    import re
    #The re (RegularExpression) is for treating strings using regular expressions

    regex = re.compile(r'([0-9]|[01][0-9]|20)\.(5|75|25)')
    #The regular expression restrict the number to be a natural number lesser than 21 and greater than -1
    #And if it is decimal 
    if not (regex.search(value)==None):
        print(regex.search(value).groups())
        print(f'The correct value is: {float(regex.search(value).groups()[0])+float("0."+regex.search(value).groups()[1])}')
    else:
        print('Couldn\'t match a good number')


correct(sys.argv[1])