<pre>
<?php
// $file_type_regex = "/\[WiML=\d+(?:\.\d+)*\]/";
// $body_content_regex = "/\[body\]\s*(?:.+\s+)+\[\/body\]/";
// $title_regex = "/\[title\].+\[\/title\]/";
class Element{
    public $parent = null;
    public $childrens = null;
    public $name;
    public $param;
    function __construct($parent, $name, $param) {
        $this->parent = $parent;
        $this->name = $name;
        $this->param = $param;
    }
}
function SyntaxError($error){
    throw new Exception($error);
}
function GetVersion($file, &$pageData) {
    global $file_type_regex;
    preg_match($file_type_regex, $file,$matches);
    $pageData["version"] = substr($matches[0],6,strlen($matches[0])-7);
}


$pageData = new Element(null, );
$file = file_get_contents("./article.wiml");
GetVersion($file, $pageData);
if($file[0] != "["){
    SyntaxError("Un excepted symbol");
}


?>
</pre>