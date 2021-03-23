# Data Cube
A generic data structure to store represent n-dimensional data as a cube in .net

Typical use-cases for this structure include:
* Representing the results of financial risk calculations where there's a desire to be able to provide summaries over axes as well as drill down

## FAQs
### Why do I need this over just a dictionary?
There can be multiple reasons:
1. Error handling.
2. Meta data about keys / consistency around keys

### My data contains information which cannot be simply summed together
In this case, there are 2 choices:

1. Use a different data structure - see our other repositories for suggestions
2. Use a custom value type for the cube which internally handles the information. 

A use-case for this might be where one wished to store the underlying cashflows which contributed to a valuation as a collection within the cube value. In the **Add** method, one would simply combine the two cashflow collections together 

### How are errors handled / why are they handled in this way?
A cube is made up of n dimensions, e.g. for the calculation of FX vega

 * Instrument Identifier
 * Currency Pair
 * Currency
 * Date
 
Errors are [effectively] stored as a collection of:

* Relevant error keys (note that it does not have to be every possible axis)
* The error message

This means that there could be multiple errors for a given point.

It's reasonable to assume that errors could break down into the following portions:

1. The instrument always fails => errors are at the identifier level
2. The instrument fails for a given perturbation (ccypair-date) => errors are at the identifier-ccypair-date level

Which leads to the following justifications:

1. Depending on the calculation engine that's used, under a failure state, it might not be known what possible ccy pairs a given identifier might be sensitive to and therefore it's impossible to know what's the full set of values which should be used (if one required every combination to be returned).
2. We can reduce the number of errors in the structure / avoid duplication by storing errors at their actual level
3. There could be multiple errors affecting a given point - e.g. an entire currency pair's perturbation may have failed and an individual trade's valuation may have failed. 

### The errors are way too general for my use-case, what do I do?
This can happen in the following circumstance:

 * Source data = {identifier, currency}
 * Errors = an individual identifier has failed and this should be reported
 * Use-case = A projection has been down over all identifiers for a given currency 

In this example, a user would see that an error was being reported for that currency but they know, through whatever means in their process, that the erroring identifier couldn't affect it. There are 2 choices here:

1. Put more information, i.e. add currency as a key, into the error which is added to the result structure
2. Externally key by currency, i.e. store one data cube per currency and only push the errors into the appropriate cubes

Which is more suitable depends on the use-case. FWIW Option #1 is more common.

### Why aren't you using a standard dictionary internally?
Initially we did. The reason we stopped doing so and moving to what appears to be a slightly bizarre approach is to reduce the memory from storing the keys as arrays. When one adds very large numbers of items to the dictionary, more memory was being used by storing the arrays than was needed in the nested approach that has been taken.

### How can one display the data?
That's up to you, we use a custom pivot table control to do this, which allow facilitates drill down, but it's your data, you get to choose how it's presented

### I have suggestions / feedback / improvements, what do I do?
Either raise an issue here or contact us.
