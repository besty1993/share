'''Test of Equivalence and Non-Inferiority
currently only TOST for paired sample
Application for example bioequivalence
Author: Josef Perktold
License: BSD-3
'''


import numpy as np
from scipy import stats
import traceback
import os.path

def tost_paired(y, x, low, upp, transform=None):
    '''test of (non-)equivalence for paired sample
    TOST: two one-sided t tests
    null hypothesis  x - y < low or x - y > upp
    alternative hypothesis:  low < x - y < upp
    If the pvalue is smaller than a threshold, then we reject the hypothesis
    that there is difference between the two samples larger than the one
    given by low and upp.
    Parameters
    ----------
    y, x : array_like
        two paired samples
    low, upp : float
        equivalence interval low < x - y < upp
    transform : None or function
        If None (default), then the data is not transformed. Given a function
        sample data and thresholds are transformed. If transform is log the
        the equivalence interval is in ratio: low < x / y < upp
    Returns
    -------
    pvalue : float
        pvalue of the non-equivalence test
    t1, pv1 : tuple of floats
        test statistic and pvalue for lower threshold test
    t2, pv2 : tuple of floats
        test statistic and pvalue for upper threshold test
    Notes
    -----
    tested on only one example
    uses stats.ttest_1samp which doesn't have a real one-sided option
    '''
    # if transform:
    #     y = transform(y)
    #     x = transform(x)
    #     low = transform(low)
    #     upp = transform(upp)
    df = x.size-1
    t1, pv1 = stats.ttest_1samp(x/y, low)
    # print((df == 13 and t1 > 2.160) or (df == 15 and t1 > 2.120))  ## .975
    # print((df == 13 and t1 > 1.771) or (df == 15 and t1 > 1.746))  ## .95
    # print((df == 13 and t1 > 1.350) or (df == 15 and t1 > 1.337))  ## .90
    # print((df == 13 and t1 > 1.079) or (df == 15 and t1 > 1.071))  ## .85
    # print(t1 > pv1 / 2 and t2 < pv2,t3 > pv3 / 2 and t4 < pv4)
    return t1, pv1 / 2.
    

if __name__ == '__main__':
    try :

        # example from http://support.sas.com/documentation/cdl/en/statug/63033/HTML/default/viewer.htm#statug_ttest_sect013.htm
        # raw = np.array('''\
        #    103.4 90.11  59.92 77.71  68.17 77.71  94.54 97.51
        #    69.48 69  72.17 101.3  74.37 79.84  84.44 96.06
        #    96.74 89.30  94.26 97.22  48.52 61.62  95.68 85.80'''.split(), float)
        raw = np.array('''\
99	40	99	2	2
56	53	56	53	43
100	89	100	100	2
68	55	62	93	0.001
71	61	92	42	8
60	64	97	85	0.001
25	64	66	69	24
67	61	100	43	20
16	18	58	11	0.001
64	66	58	63	47
70	70	77	55	1
79	85	82	71	0.001
41	43	44	35	0.001
59	47	74	14	0.001
71	72	79	62	39
37	25	95	54	0.001

'''.split(), float)
        a = []
        b = []
        c = []
        d = []
        e = []
        for i in range(0, raw.size):
            if i % 5 == 0:
                a.append(raw[i])
            elif i % 5 == 1:
                b.append(raw[i])
            elif i % 5 == 2:
                c.append(raw[i])
            elif i % 5 == 3:
                d.append(raw[i])
            else:
                e.append(raw[i])
        print(a, b, c, d, e)
        a = np.array(a).T
        b = np.array(b).T
        c = np.array(c).T
        d = np.array(d).T
        e = np.array(e).T
        total = [a, b, c, d, e]
        maxVal = np.argmax([np.average(a), np.average(b), np.average(c), np.average(d), np.average(e)])
        low = 0.8
        high = 1.25
        t1, pv1=tost_paired(total[maxVal], a, low, high, transform=np.log)
        t2, pv2 =tost_paired(total[maxVal], b, low, high, transform=np.log)
        t3, pv3 =tost_paired(total[maxVal], c, low, high, transform=np.log)
        t4, pv4 =tost_paired(total[maxVal], d, low, high, transform=np.log)
        t5, pv5 =tost_paired(total[maxVal], e, low, high, transform=np.log)

        file = None
        file = open("aa.csv", "a+")
        file.write(str(t1) + "," +
                   str(t2) + "," +
                   str(t3) + "," +
                   str(t4) + "," +
                   str(t5) + "\n")
        file.close()

        # x, y = raw.reshape(-1,2).T
        # print (x,y)
        # print (tost_paired(y, x, 0.8, 1.25, transform=np.log))
    except:
        traceback.print_exc()