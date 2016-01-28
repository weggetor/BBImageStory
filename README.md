# BBImageStory
## DNN Module for creating image stories, step-by-step guides or image galleries with subtexts
BBImageStory is a DNN Module which let you create step-by-step guides in an easy way:
![Sample1](http://www.bitboxx.net/portals/0/images/bbimagestory/Sample1.png)

Even multiple news with multiple pictures incl. picture title & text is possible:
![Sample2](http://www.bitboxx.net/portals/0/images/bbimagestory/Sample2.png)

## DNN SPA module written with AngularJS
This module is of the new SPA type (single page application) and runs only in DNN 8 and up. It comes with a lot of included javascript libraries. These are libs (e.g. AngularJS itself) could be used platform wide and have the advantage that not every module is in need to include them. Sharing is better (and less error prone) than loading multiple times!

## Getting started

After installation of the module (which is done in the same way as every DNN module) and adding the module to a page, you first have to fill out the new QuickSettings (new in DNN 8)
### Quicksettings
![QuickSettings](http://www.bitboxx.net/portals/0/images/bbimagestory/QuickSettings.png)
Define here:
1. Width of the images in display (the are automatically resized by the new dnnImageHandler. No need to brings them in the right dimension before uploading!)
2. The scope: Define which stories should be dieplayed in this module: All stories, only the module stories or all portal stories.
3. Which templkate should be used for list display and view display. At installation time there is only 1 for every type present, but you can create as much designs as you want when you click the button "Edit templates".

### Create Story
After saving your settings you see the following picture:
![Create Story](http://www.bitboxx.net/portals/0/images/bbimagestory/CreateStory.png)
If you click the "Create story" button or the red pencil button in the actionmenu of the module which changes mode between edit and display (red pencil is display mode, green one is edit mode), you enter the edit screen where you are able to create, update and delete stories:

### Edit Stories
![Edit Stories](http://www.bitboxx.net/portals/0/images/bbimagestory/EditStories.png)
In this sample case you see three stories here which could be edit (green pencil button), viewed (blue eye button) or deleted (red cross button). You may also create a new story by clicking the blue plus button. All stories within the selected scope are shown. 
> Please be aware that after changing the scope in Quicksettings it may be needed to hit Shift-F5 in your browser to refresh some cached files!

### New Story
![New story](http://www.bitboxx.net/portals/0/images/bbimagestory/NewStory.png)
A story has a **title** which should be plain text and the **story** itself which could be html or also plain text. If you enter a valid date in **Start Date** or **End Date**, this story is shown only between these two dates.

After saving this story successfully, you are able to add images:
### Upload Images
![Upload Images](http://www.bitboxx.net/portals/0/images/bbimagestory/Upload.png)
After the successful upload, you see all the uploaded images attached to the story:

### Edit Story Images
You can change the image order by dragging and dropping the images. Every image has its own **title** and **content** like the story itself. The first image is shown when you open the story in view mode or when this story is shown in list view 
![Story with images](http://www.bitboxx.net/portals/0/images/bbimagestory/EditStory.png)

### Export and Import
You are able to export and import the content of the module. 
![Export and Import](http://www.bitboxx.net/portals/0/images/bbimagestory/Export.png)
When selecting Export, all the stories in shown in edit mode (depending on the selected scope) are written into one export file together with their templates and all included images. So it is easy to export the whole story and import it in another module or on another DNN installation. The module comes with 3 samples (installed in Templates folder on portal 0):

1. Yahoo: 3 sample stories copied from Yahoo news
2. iFixit: Step by step guide how to change the battery in an iphone 6
3. MickeyMouse: Short Mickey comic strip

### Localization
In a multilanguage portal you can edit the stories for every language seperately. Select the language you want to edit as portal language and then go to edit mode. The actual selected language is also shown in the editor:
![Language marker](http://www.bitboxx.net/portals/0/images/bbimagestory/Language.png)
Saving the story always saves the text to specific language tables!

### Template editing
You can edit your templates for view and list from the Quicksettings. After clicking the "Edit templates" button, you are in template editor mode:
> Only people with **admin** rights are able to edit templates! 
> Editing stories is able with **edit** rights
![Editing templates](EditTemplate.png)
After selecting the template type (*View*, *List* or *Stylesheet*) you can load one of your templates in the Template File dropdown list. You can always create a new one by clicking the **Add Template** button and save it under a new name. The templates are pure html with placeholders in AngularJS syntax. See the existing templates for samples.
