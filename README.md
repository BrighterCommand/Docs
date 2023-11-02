# Brighter and Darker Documentation

Documentation for the [Brighter](https://github.com/BrighterCommand/Brighter) and [Darker](https://github.com/BrighterCommand/Darker) projects.

## GitHub
The documentation can be used as is, by cloning this [repository](https://github.com/BrighterCommand/Docs) and viewing the markdown files.

### GitHub Organization

The markdown files in the source are organized by version. We support two versions at any time.

\source
* shared: Markdown files that work with all versions
* N: Markdown files that are specific to a version

\contents
* N: Markdown file for a version (merges \source\shared && \source\N)

## GitBook
For convenience the documentation is made available via [GitBook](https://brightercommand.gitbook.io/paramore-brighter-documentation/).

### GitBook Organization
GitBook uses the versions from \contents

In the table of contents, the first page group for the documents belonging to a version will be "VERSION N" where N is the version number. That page group has one entry only, a version header. This holds metadata about the version.

Page groups that follow are documentation specific to that version. Always ensure that you are in the correct part of the documentation for the version that you are working with.


### GitBook Limitations
GitBook's table of contents is a series of page groups grouped by header. There are limitations to how we can organize this. The markdown header level is ignored by GitHub, and headers without page groups are not displayed. For this reason the best we can do is to "bracket" a version with version page groups.





