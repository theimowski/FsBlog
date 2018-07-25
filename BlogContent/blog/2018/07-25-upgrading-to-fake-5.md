@{
    Layout = "post";
    Title = "Upgrading to FAKE 5";
    Date = "2018-07-25T10:48:42";
    Tags = "F#, FAKE, Paket";
    Description = "";
}

// TODO
Short description

<!--more-->

## Motivation

## The guide

* https://fake.build/fake-migrate-to-fake-5.html#Use-the-new-FAKE-API

1. Update to legacy FAKE 5,
2. Fix all the (obsolete) warnings,
3. Use new version of FAKE 5.

## Migration in practice 

### Step 1: Update to legacy FAKE 5

This one is a no-brainer: FAKE 5 is still released as a stand-alone package to mitigate the migration.
Because I use paket, it was just a matter of invoking `paket update`.
This version of FAKE package is called *legacy*, as from FAKE 6 onwards that package will no longer be available.

> NOTE: I encourage to use a separate `Build` Paket Group for all dependencies (including FAKE) that are used just for the build process - this gives you a clearer picture of your overall dependencies.

### Step 2: Fix all the (obsolete) warnings

You'll probably spend most time here.

### Step 3: Use new version of FAKE 5

## Summary

## Resources

* https://github.com/fsharp/FAKE
* http://fake.build/