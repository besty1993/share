'''Test of Equivalence and Non-Inferiority
currently only TOST for paired sample
Application for example bioequivalence
Author: Josef Perktold
License: BSD-3
'''


import numpy as np
from scipy import stats

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
    if transform:
        y = transform(y)
        x = transform(x)
        low = transform(low)
        upp = transform(upp)
    t1, pv1 = stats.ttest_1samp(x - y, low)
    t2, pv2 = stats.ttest_1samp(x - y, upp)
    return max(pv1, pv2)/2., (t1, pv1 / 2.), (t2, pv2 / 2.)


if __name__ == '__main__':

    #example from http://support.sas.com/documentation/cdl/en/statug/63033/HTML/default/viewer.htm#statug_ttest_sect013.htm
    raw = np.array('''\
       73	99
74	58
100	54
87	74
98	81
83	100
85	72
80	74
68	56
51	54
74	63
70	66
68	62
60	55
74	63
76	18
'''.split(), float)

    x, y = raw.reshape(-1,2).T
    print(x,y)
    print (tost_paired(y, x, 0.8, 1.20, transform=np.log))
    print(tost_paired(x, y, 0.8, 1.20, transform=np.log))