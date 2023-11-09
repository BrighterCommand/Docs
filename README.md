# Brighter and Darker Documentation

Documentation for the [Brighter](https://github.com/BrighterCommand/Brighter) and [Darker](https://github.com/BrighterCommand/Darker) projects.

## GitHub
The documentation can be used as is, by cloning this [repository](https://github.com/BrighterCommand/Docs) and viewing the markdown files.

### GitHub Organization

The markdown files in the source are organized by version. We support two versions at any time.

<strong>\source</strong>
* shared: Markdown files that work with all versions
* N: Markdown files that are specific to a version

<strong>\contents</strong>
* N: Markdown file for a version (merges \source\shared && \source\N)

### Building the Documentation

We use the [Rewind](https://github.com/BrighterCommand/Rewind) tool to build <strong>\contents</strong> from  <strong>\source</strong>. See the [README](https://github.com/BrighterCommand/Rewind/blob/main/README.md) for more information on how to build the documentation, in particular how the <strong>.toc.yaml</strong> file works.

Typically, you can build the documentation by running the following command at the root:

```bash
bin/Rewind makebook source .
```

## GitBook
### Sources
For convenience the documentation is made available via [GitBook](https://brightercommand.gitbook.io/paramore-brighter-documentation/).

GitBook uses the pages from <strong>\contents</strong>

### Structure

<strong>VERSION N</strong>
* Version Info

<strong>Page Group</strong>
* Documentation File
* Documentation File

... More Groups

<strong>END OF VERSION N</strong>
* Version Info

### How to Use

In the table of contents, the first page group for the documents belonging to a version will be "VERSION N" where N is the version number. That page group has one entry only, a version header. This holds metadata about the version.

Page groups that follow are documentation specific to that version. 

The page groups for that version are terminated by a page group "END OF VERSION N" where N is the version number. That page group has one entry only, a version footer. This holds metadata about the version. 

Always ensure that you are in the correct part of the documentation for the version that you are working with.

### GitBook Limitations
GitBook's table of contents is a series of page groups grouped by header (level is ignored). There are limitations to how we can organize this. For this reason the best we can do is to "bracket" a version with version page groups.
