story(main)
{
  local
  {
    @dlgId(0);
    @index(0);      
    @dlgItem(0);
    @tag(0);
    @isFinish(0);
  };
  onmessage("start")
  {
  	log("StoryDlg start");
    @window.active=changetype(0,"bool");
  };
  onmessage("show_dlg")
  {
    publishgfxevent("ui_hide","ui");
    @isFinish=0;
    @dlgId = $0;
    @index = 0;
    localnamespacedmessage("show_dlg_item",@dlgId,@index);
    log("First,show_dlg_item:{0} {1}",@dlgId,@index);
    while(@isFinish<=0){
      waitlocalnamespacedmessage("on_click")set("@tag",0)timeoutset(5000,"@tag",1);
      if(@tag>0 && @isFinish<=0){
        inc(@index);
        localnamespacedmessage("show_dlg_item",@dlgId,@index);
        log("Timeout,show_dlg_item:{0} {1}",@dlgId,@index);
      };
    };
    clearmessage("show_dlg");
  };  
  onnamespacedmessage("on_click")
  {
    inc(@index);
    localnamespacedmessage("show_dlg_item",@dlgId,@index);
    log("on_click,show_dlg_item:{0} {1}",@dlgId,@index);
  };
  onnamespacedmessage("show_dlg_item")
  {
    @dlgId = $0;
    @index = $1;
    
    @dlgItem = getdialogitem(@dlgId,@index);
    if(isnull(@dlgItem)){
      if(@isFinish<=0){
        @window.active=changetype(0,"bool");
        publishgfxevent("ui_show","ui");
        firemessage("dialog_over:"+@dlgId);
      };
      log("Finish,show_dlg_item:{0} {1}",@dlgId,@index);
      @isFinish=1;
    }else{
      if(@dlgItem.leftOrRight>0){
        //@left.active=changetype(0,"bool");
        //@right.active=changetype(1,"bool");
        @right_Image.sprite=getactoricon(@dlgItem.speaker);
      }else{
        //@left.active=changetype(1,"bool");
        //@right.active=changetype(0,"bool");
        @left_Image.sprite=getactoricon(@dlgItem.speaker);
      };        
      @text_Text.text = @dlgItem.dialog;
      @window.active=changetype(1,"bool");
    };
  };
};