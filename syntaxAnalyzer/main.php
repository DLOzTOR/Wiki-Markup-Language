<?php 
require_once __DIR__."/WiMLToHTML.php";
$file = file_get_contents("./article.wiml");
echo WiMLToHTML::Convert($file);
?>