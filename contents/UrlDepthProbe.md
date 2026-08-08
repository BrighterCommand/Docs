# URL Depth Probe

> **Reference** · Applies to **Brighter V10**

This page is a temporary measurement for Spec 010 and will be removed within minutes of
being published. It exists to establish, against the live site rather than by inference,
whether a page nested three levels deep in `SUMMARY.md` publishes at a four-segment URL.

## URL Depth Probe Method

Spec 010's shape rule **S3** caps a published path at three segments, on the stated grounds
that three is the deepest the live site is *known* to work at and four had never been
tested. That is an evidence boundary rather than a platform limit, and it was costing
navigation quality in two places, so it is being measured instead of assumed.

This page is nested under *Azure Archive Provider Configuration*, which itself publishes at
three segments. The predicted path is therefore:

```text
guaranteed-at-least-once/azureblobarchiveprovider/azureblobconfiguration/urldepthprobe
```

A page that has never existed at any path is used deliberately, so that no automatic
redirect can mask the result — the same reasoning that made Spec 010's D0 experiment a
measurement rather than a hope. No existing page moves, so no real URL churns and no
redirect is cached against content a reader might want.

The result is read from `sitemap-pages.xml` and from the response fingerprint, never from
the status code: every cached response on this site reports `200`, and the reliable tell is
that no genuine page response carries a `location:` header.

## Further Reading

- [Basic Concepts](/contents/BasicConcepts.md)
