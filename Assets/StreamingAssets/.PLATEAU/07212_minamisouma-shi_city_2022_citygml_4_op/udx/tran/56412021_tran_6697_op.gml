<?xml version="1.0" encoding="UTF-8"?>
<core:CityModel xmlns:brid="http://www.opengis.net/citygml/bridge/2.0" xmlns:tran="http://www.opengis.net/citygml/transportation/2.0" xmlns:frn="http://www.opengis.net/citygml/cityfurniture/2.0" xmlns:wtr="http://www.opengis.net/citygml/waterbody/2.0" xmlns:sch="http://www.ascc.net/xml/schematron" xmlns:veg="http://www.opengis.net/citygml/vegetation/2.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:tun="http://www.opengis.net/citygml/tunnel/2.0" xmlns:tex="http://www.opengis.net/citygml/texturedsurface/2.0" xmlns:gml="http://www.opengis.net/gml" xmlns:app="http://www.opengis.net/citygml/appearance/2.0" xmlns:gen="http://www.opengis.net/citygml/generics/2.0" xmlns:dem="http://www.opengis.net/citygml/relief/2.0" xmlns:luse="http://www.opengis.net/citygml/landuse/2.0" xmlns:uro="https://www.geospatial.jp/iur/uro/3.1" xmlns:xAL="urn:oasis:names:tc:ciq:xsdschema:xAL:2.0" xmlns:bldg="http://www.opengis.net/citygml/building/2.0" xmlns:smil20="http://www.w3.org/2001/SMIL20/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:smil20lang="http://www.w3.org/2001/SMIL20/Language" xmlns:pbase="http://www.opengis.net/citygml/profiles/base/2.0" xmlns:core="http://www.opengis.net/citygml/2.0" xmlns:grp="http://www.opengis.net/citygml/cityobjectgroup/2.0" xsi:schemaLocation="https://www.geospatial.jp/iur/uro/3.1 ../../schemas/iur/uro/3.1/urbanObject.xsd http://www.opengis.net/citygml/2.0 http://schemas.opengis.net/citygml/2.0/cityGMLBase.xsd http://www.opengis.net/citygml/landuse/2.0 http://schemas.opengis.net/citygml/landuse/2.0/landUse.xsd http://www.opengis.net/citygml/building/2.0 http://schemas.opengis.net/citygml/building/2.0/building.xsd http://www.opengis.net/citygml/transportation/2.0 http://schemas.opengis.net/citygml/transportation/2.0/transportation.xsd http://www.opengis.net/citygml/generics/2.0 http://schemas.opengis.net/citygml/generics/2.0/generics.xsd http://www.opengis.net/citygml/relief/2.0 http://schemas.opengis.net/citygml/relief/2.0/relief.xsd http://www.opengis.net/citygml/cityobjectgroup/2.0 http://schemas.opengis.net/citygml/cityobjectgroup/2.0/cityObjectGroup.xsd http://www.opengis.net/gml http://schemas.opengis.net/gml/3.1.1/base/gml.xsd http://www.opengis.net/citygml/appearance/2.0 http://schemas.opengis.net/citygml/appearance/2.0/appearance.xsd">
	<gml:boundedBy>
		<gml:Envelope srsDimension="3" srsName="http://www.opengis.net/def/crs/EPSG/0/6697">
			<gml:lowerCorner>37.5151740446420 141.0098555127650 -0.0000100000000</gml:lowerCorner>
			<gml:upperCorner>37.5258887275460 141.0251015981540 0.0000100000000</gml:upperCorner>
		</gml:Envelope>
	</gml:boundedBy>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_64ecbf91-f0c9-4748-9d73-d4fce7da40cb">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51699242516182 141.0148699538401 0 37.51701129334842 141.01486865976426 0 37.51700932692667 141.0148234824351 0 37.51700144040197 141.01480534819467 0 37.51700289130687 141.01479154550748 0 37.51696860324414 141.01477737952933 0 37.51695947976713 141.01481218643863 0 37.51693764392051 141.014828573744 0 37.516937213213495 141.01487225511076 0 37.516958735796585 141.0148725899695 0 37.51699242516182 141.0148699538401 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_391f952c-0b61-40de-9f21-d110069be646">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51681683442517 141.01275195217653 0 37.51718673259318 141.01264946948226 0 37.5171760570613 141.01258873895176 0 37.51680474058206 141.01269159439872 0 37.51655545037485 141.01278012454333 0 37.516568938541276 141.01283998599337 0 37.51681683442517 141.01275195217653 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_65e939b9-f422-470e-837f-8fe2111b1dc1">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51718673259318 141.01264946948226 0 37.51720969072913 141.01264310919572 0 37.517199012649726 141.0125823627704 0 37.51719573274279 141.01257497414625 0 37.5171760570613 141.01258873895176 0 37.51718673259318 141.01264946948226 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_054fd47d-6a28-4cd2-9fe5-b0c2c906c0e0">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51721728708904 141.01399493686287 0 37.51732203862843 141.0135842621113 0 37.51728775642459 141.01357048697884 0 37.517191820537406 141.01394657244447 0 37.517183099548426 141.0139807105727 0 37.5170371867679 141.0145157332087 0 37.51696860324414 141.01477737952933 0 37.51700289130687 141.01479154550748 0 37.51707137310996 141.01453007281435 0 37.51721728708904 141.01399493686287 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_6eb74031-a558-4cda-9cbb-286f5923c65b">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51739931566495 141.01358751553792 0 37.51746102472704 141.0135707275697 0 37.51745786047682 141.01353572333397 0 37.517454156335496 141.01348261123582 0 37.517332704093654 141.01342955751872 0 37.517323710914354 141.01346200485423 0 37.51728775642459 141.01357048697884 0 37.51732203862843 141.0135842621113 0 37.517335547178156 141.01356694116586 0 37.51734162391254 141.01356296453343 0 37.51734731541903 141.0135614704911 0 37.5173563244381 141.01356161239624 0 37.51738428144978 141.013568161289 0 37.51739188259942 141.0135739370647 0 37.51739454800407 141.01357771203953 0 37.51739931566495 141.01358751553792 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_6498e066-56e8-41ee-b903-3c91f4934485">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51732592856302 141.02454923148628 0 37.51736619775221 141.0245053021121 0 37.517346577689146 141.0244673209979 0 37.517288540315434 141.02450406755082 0 37.51726119183099 141.02454605639767 0 37.51727999294181 141.0245653561707 0 37.51729198051707 141.02458285421199 0 37.51732592856302 141.02454923148628 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_b0dec150-44b0-406f-8ee4-8c9816cb4d8d">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.517454156335496 141.01348261123582 0 37.517489804453355 141.0134679013668 0 37.517484665295164 141.01342313760628 0 37.51744946295776 141.01341127108086 0 37.517349834222735 141.0133677499821 0 37.517332704093654 141.01342955751872 0 37.517454156335496 141.01348261123582 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_c8466b89-3b65-4e20-a3a3-d0876167d870">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51740480256746 141.01258727349637 0 37.51750806142234 141.01256027899885 0 37.51749186191252 141.01248615598482 0 37.51741461430234 141.0125160485922 0 37.51739182009076 141.01252485266286 0 37.517202778644325 141.01258131685208 0 37.517199012649726 141.0125823627704 0 37.51720969072913 141.01264310919572 0 37.51721376209882 141.01264198098374 0 37.51740480256746 141.01258727349637 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_6c2ba3d0-06be-4e07-87ab-f3cab6deb4e1">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51757700760173 141.01274812719618 0 37.51760282300722 141.0126713848619 0 37.517544652633624 141.01264060529329 0 37.517518648032144 141.01271824957024 0 37.51749565589306 141.01279198197525 0 37.517462352750655 141.0129057099818 0 37.51743261243556 141.0130235663358 0 37.517413390038215 141.01310776504434 0 37.51739879751726 141.01317948019044 0 37.51738329758242 141.0132428100719 0 37.51737456097361 141.01327853181752 0 37.517349834222735 141.0133677499821 0 37.51744946295776 141.01341127108086 0 37.51744757973808 141.01329314309544 0 37.51745012436316 141.0132729345079 0 37.517459871280025 141.01319921997208 0 37.517474273464174 141.01312851986415 0 37.51749293386922 141.01304646154517 0 37.51752201533389 141.01293142276185 0 37.517554574058494 141.01282005850334 0 37.51757700760173 141.01274812719618 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_7d9b0a9d-12fe-4d37-bf30-224ff68850e7">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51765174915533 141.0125812054069 0 37.51770777655326 141.01249272128877 0 37.517659260089296 141.01243358716076 0 37.51749186191252 141.01248615598482 0 37.51750806142234 141.01256027899885 0 37.51752403197821 141.01256709137158 0 37.517533242378995 141.01257413671695 0 37.51754097180722 141.01258511799605 0 37.51754510214323 141.0125955901479 0 37.517547668191646 141.0126093181945 0 37.51754775914813 141.01261825619778 0 37.517544652633624 141.01264060529329 0 37.51760282300722 141.0126713848619 0 37.51765174915533 141.0125812054069 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_693bf23c-0ac1-4921-80ea-0c1496fcc112">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51745297846419 141.0147958143887 0 37.5177759899766 141.01468331562685 0 37.51777119618445 141.01463803104667 0 37.51744337530158 141.01475220527925 0 37.51734356615668 141.01478684605488 0 37.51729936868326 141.01479895329763 0 37.517270703156534 141.01480324897818 0 37.5171989068649 141.01481047764193 0 37.51700932692667 141.0148234824351 0 37.51701129334842 141.01486865976426 0 37.5172013313323 141.01485562384704 0 37.51727428122551 141.01484827880344 0 37.517305359758524 141.01484362187324 0 37.51735221629491 141.01483078623227 0 37.51745297846419 141.0147958143887 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_abc9e968-ae9b-4330-b46d-08b121ecfba8">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51779455551321 141.014614002415 0 37.51784813688944 141.01457239263294 0 37.517820282507785 141.01451586104983 0 37.51779275695662 141.0144571695466 0 37.51770642125754 141.01425151136726 0 37.517629609123716 141.01406659180478 0 37.51759076398537 141.0139684691863 0 37.51755204919645 141.01387532605375 0 37.51751063572213 141.0137637017628 0 37.51748397102981 141.01368194764427 0 37.517470475384016 141.01362585325535 0 37.51746102472704 141.0135707275697 0 37.51739931566495 141.01358751553792 0 37.51740936529504 141.01364581800166 0 37.517424508209544 141.01370838619295 0 37.517452832593214 141.01379536996913 0 37.51749547338197 141.01391040716004 0 37.517534629549345 141.01400446216326 0 37.517573385699755 141.0141024702022 0 37.517650549092316 141.01428830016718 0 37.517737587351675 141.01449577925115 0 37.51776626143074 141.01455675126047 0 37.51779455551321 141.014614002415 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_42c20a56-2049-49ca-af37-93b62a3ab4f1">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.5177595786593 141.0241850877366 0 37.51777021953418 141.02417530217664 0 37.51774340928156 141.0241444464326 0 37.517346577689146 141.0244673209979 0 37.51736619775221 141.0245053021121 0 37.5177595786593 141.0241850877366 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_131f2733-4440-4413-b69a-9ba3ad19b17c">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51783529066282 141.01468193387274 0 37.51788311295076 141.014630199429 0 37.517856401965844 141.0145891676469 0 37.51784813688944 141.01457239263294 0 37.51779455551321 141.014614002415 0 37.51777119618445 141.01463803104667 0 37.5177759899766 141.01468331562685 0 37.51783529066282 141.01468193387274 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_1f871d15-f7c4-466d-a37e-3e5fcde2351e">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.517489804453355 141.0134679013668 0 37.51871643928126 141.01324706184187 0 37.51871130005261 141.01320229735907 0 37.517484665295164 141.01342313760628 0 37.517489804453355 141.0134679013668 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_998518e6-b436-45fe-ac5e-89a31b760b26">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51874205846293 141.01677215209347 0 37.518772518200706 141.01676190462658 0 37.51873116104493 141.01656813701766 0 37.518724313414744 141.01654589412027 0 37.518713165304135 141.0165178751342 0 37.518703936788306 141.01649737294065 0 37.51869102136898 141.01647622429292 0 37.51867597954863 141.01645596855076 0 37.518660186190374 141.01644012631354 0 37.51864410080957 141.01642618106578 0 37.51861743050938 141.0164054362997 0 37.5185946358652 141.0163903308251 0 37.51857361907467 141.01637845367196 0 37.51855689422414 141.0163696974738 0 37.518534583718406 141.01635996054756 0 37.518506515364905 141.01635195059433 0 37.51847361519352 141.01634560191852 0 37.518412577520905 141.01634255333394 0 37.51841186533889 141.01634249910265 0 37.518342552297106 141.01634297710845 0 37.51827852477149 141.0163465136833 0 37.518216157343424 141.01635253121535 0 37.51816585122257 141.01635981383734 0 37.5181003685527 141.01637009211075 0 37.51804958733487 141.01637892829223 0 37.518000580722266 141.01638870876945 0 37.5179623507516 141.01639757342562 0 37.51792633138893 141.01640793224806 0 37.51789488729991 141.01641939271104 0 37.51784880256037 141.0164397169629 0 37.51774162100898 141.01648942692353 0 37.51746947875469 141.01662433632248 0 37.51737670270644 141.01666975976505 0 37.517279414549456 141.01671047388675 0 37.517092434013016 141.0167712189468 0 37.516962272747406 141.01680773716575 0 37.516916373357446 141.01681675185168 0 37.51687764650865 141.0168203482428 0 37.516838668722905 141.01681751544868 0 37.516795560736895 141.0168114274399 0 37.51658127398197 141.01677030392185 0 37.516576509064805 141.0168094370672 0 37.51679141362389 141.01685068017866 0 37.51683599273298 141.01685697583187 0 37.51687789947056 141.01686002232756 0 37.51691997861861 141.01685611466027 0 37.51696815402215 141.01684665292453 0 37.517099824291414 141.01680971064798 0 37.51728836060048 141.0167484608456 0 37.517387424066506 141.01670700332275 0 37.51748099785643 141.0166611898082 0 37.51775287663975 141.01652641097795 0 37.51785949242318 141.01647696362136 0 37.51790451555096 141.01645710791843 0 37.51793425405163 141.01644626872556 0 37.517968737217515 141.01643635172542 0 37.51800592045186 141.0164277296093 0 37.51805422736207 141.01641808856087 0 37.518104488677686 141.0164093426124 0 37.518169613069524 141.01639912093287 0 37.51821917204538 141.01639194644923 0 37.51828042650555 141.01638603694212 0 37.51834333161708 141.0163825618641 0 37.51841099383417 141.01638209515932 0 37.51847058334393 141.01638507226716 0 37.51850061833037 141.0163908677987 0 37.518525885241196 141.01639807787208 0 37.51854563803968 141.01640669975697 0 37.51856107575439 141.01641478065764 0 37.51858078979259 141.0164259223125 0 37.51860174768138 141.016439811003 0 37.51862682651921 141.01645931797032 0 37.518641362946575 141.0164719197507 0 37.51865449077762 141.0164850875311 0 37.51866692056895 141.01650182576128 0 37.51867756919785 141.01651926324928 0 37.518685316056796 141.01653647165867 0 37.5186954375901 141.01656191106107 0 37.51870113853635 141.01658043219973 0 37.51874205846293 141.01677215209347 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_a2b29f3a-234d-4f86-bbe9-64f5e15f74c1">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.518803355438955 141.01686135397463 0 37.51881579252008 141.01685704021975 0 37.51880067577015 141.01678834278036 0 37.518772518200706 141.01676190462658 0 37.51874205846293 141.01677215209347 0 37.518736461141174 141.01681108882528 0 37.51875236043826 141.01687992074415 0 37.518803355438955 141.01686135397463 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_14b5e635-0887-4802-82b7-c2f8521a4c5e">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51882604199937 141.0154315677051 0 37.51888596567254 141.01537532712186 0 37.51887043252874 141.01535003660183 0 37.51883272404544 141.0153005046647 0 37.51880729224554 141.01527483177932 0 37.51877254510947 141.01524271101388 0 37.518735492603014 141.01521406072382 0 37.51869476248788 141.01518554476118 0 37.51866811427684 141.0151697736478 0 37.518598140810994 141.01513360186783 0 37.51850799334965 141.01508568657928 0 37.518438072431714 141.01504930311748 0 37.518284265747965 141.01496451298934 0 37.518174236015355 141.01490441693315 0 37.518126747463015 141.01487541356204 0 37.51810244406254 141.0148584375802 0 37.51805288928171 141.01481737795845 0 37.518004669451585 141.01477374442666 0 37.51795203334991 141.01472121782217 0 37.51791021935461 141.01466773070266 0 37.51788311295076 141.014630199429 0 37.51783529066282 141.01468193387274 0 37.51786435301056 141.0147220977774 0 37.51790972645884 141.01478005276462 0 37.51796655377202 141.01483694409717 0 37.518015220634666 141.01488092406152 0 37.51806966422799 141.01492614902676 0 37.51809759957112 141.01494566193318 0 37.518147850042034 141.0149763525577 0 37.51825900798482 141.01503706490715 0 37.51841258162576 141.01512172704327 0 37.518498950676104 141.0151699447041 0 37.51855012740305 141.0151990099826 0 37.51861043168895 141.01523527814243 0 37.51863927460069 141.01525323322 0 37.518667310874825 141.0152722887155 0 37.51870142798436 141.01529785648208 0 37.51873350883595 141.01532449396603 0 37.51876679886722 141.0153563542061 0 37.51879397295073 141.0153870998784 0 37.51881471261606 141.01541254039688 0 37.51882604199937 141.0154315677051 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_46f1dee2-49a7-48e5-b23b-99baedab4150">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.518913757998625 141.0154208225573 0 37.51892114701392 141.01540727385182 0 37.5189029766695 141.01536999589936 0 37.51888596567254 141.01537532712186 0 37.51882604199937 141.0154315677051 0 37.51883137218121 141.01544051838698 0 37.518847838785135 141.0154731902159 0 37.518913757998625 141.0154208225573 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_874a2d34-46c6-4a5b-884c-73c8de16d14e">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51881579252008 141.01685704021975 0 37.51894341388238 141.01681277406485 0 37.51892824376305 141.01674409490434 0 37.51880067577015 141.01678834278036 0 37.51881579252008 141.01685704021975 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_011146a9-f2d9-4a87-837b-6c6b4f134f3f">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.518973923952274 141.015358830011 0 37.51898397829402 141.01535355871366 0 37.51896924164024 141.01531350682959 0 37.51895298329322 141.0153242119852 0 37.518933083590284 141.0153417828314 0 37.5189029766695 141.01536999589936 0 37.51892114701392 141.01540727385182 0 37.518943558132435 141.01538275163168 0 37.518973923952274 141.015358830011 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_565f97e5-dd89-4f3e-afa9-7147812ff563">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51898397829402 141.01535355871366 0 37.51901232706302 141.01533896054815 0 37.5189975915366 141.01529879554604 0 37.51896924164024 141.01531350682959 0 37.51898397829402 141.01535355871366 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">3</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_473b8c94-accf-4b5b-8ae6-11c9d8aca426">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51900368417503 141.01529323549445 0 37.51903041610246 141.01528767637194 0 37.51902680978206 141.01526034204326 0 37.518998643225714 141.0150136282344 0 37.518972445954475 141.01478617661755 0 37.51894372047673 141.01455020105269 0 37.51893319340508 141.01447661810928 0 37.51892654460669 141.01442968027925 0 37.51891871370748 141.01437480519863 0 37.51888255743868 141.01405285236353 0 37.518851871113924 141.01381458399098 0 37.51882687105616 141.01361136041393 0 37.51879563715858 141.01335566281824 0 37.518786805229084 141.0132836905989 0 37.518784893626695 141.01326759702476 0 37.51874648858331 141.01291789473592 0 37.518739532180035 141.01285658569654 0 37.51873488420109 141.01279870669603 0 37.51873421261065 141.01277573216478 0 37.518735979951984 141.0127611671248 0 37.51874243790497 141.01273706046882 0 37.51875960949651 141.01268676479452 0 37.51880831814257 141.0125642273573 0 37.518828883732624 141.01251196237354 0 37.51880341130088 141.01249616317077 0 37.51878338713565 141.01255240954345 0 37.51873723604239 141.01267148039338 0 37.5187161386196 141.01272691791428 0 37.5187095736349 141.01275271972168 0 37.51870674526754 141.01277428167484 0 37.5187081836148 141.01280168006147 0 37.518712297829595 141.01285887190105 0 37.518718728377564 141.0129187020413 0 37.518758172690234 141.01327260611765 0 37.51880014784652 141.0136165955967 0 37.51882523798674 141.01381982052195 0 37.518855837604825 141.01405774807574 0 37.51889198933778 141.014380153219 0 37.518917083927754 141.0145557766429 0 37.51894572496344 141.01479118517778 0 37.51897192449 141.0150184105039 0 37.51900008764197 141.01526546354242 0 37.51900368417503 141.01529323549445 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_f0476f7a-0081-4c04-a555-5c5ae1ed33d4">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.518954485517064 141.01671498422283 0 37.519023092236836 141.0167022667831 0 37.51900815213057 141.01657465395158 0 37.51900554972352 141.01651963491884 0 37.518993563632485 141.01612407968466 0 37.51898736361829 141.0157444531366 0 37.518982836462726 141.01566564787754 0 37.518973844028174 141.01560984931768 0 37.51896259316136 141.01556363061286 0 37.51894093186981 141.01549299403973 0 37.51892739279889 141.01545482758004 0 37.518913757998625 141.0154208225573 0 37.518847838785135 141.0154731902159 0 37.518859820764135 141.01549696317193 0 37.51887906250346 141.01554602284477 0 37.51889462185084 141.01559389320485 0 37.51890388103085 141.0156320486973 0 37.51891176909573 141.01568115551152 0 37.51891805489143 141.01574630054395 0 37.518924248108846 141.01612660537253 0 37.51893621719986 141.01652385682044 0 37.51893904227945 141.01658363049413 0 37.518954485517064 141.01671498422283 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_cc639848-d516-4b34-8a59-a8fcc715a88e">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.519019167118785 141.01533974719788 0 37.51904079154204 141.01531937319345 0 37.51903718203439 141.01531333428585 0 37.51903141608024 141.01529525670702 0 37.51903041610246 141.01528767637194 0 37.51900368417503 141.01529323549445 0 37.5189975915366 141.01529879554604 0 37.51901232706302 141.01533896054815 0 37.519019167118785 141.01533974719788 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_72a879c9-e8ce-45e6-8841-24e90e7f503c">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51898047599153 141.01682783926964 0 37.51904869927434 141.01679952747074 0 37.519023092236836 141.0167022667831 0 37.518954485517064 141.01671498422283 0 37.518949043264065 141.01672756806755 0 37.51894457104783 141.0167332667151 0 37.51892824376305 141.01674409490434 0 37.51894341388238 141.01681277406485 0 37.51896549841537 141.01681187855132 0 37.51897204665288 141.01681481006747 0 37.51897588085372 141.01681882994635 0 37.51898047599153 141.01682783926964 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_1b63e83e-5c87-46fe-9060-7c4bbdf1817f">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.519162107401556 141.0177825717288 0 37.519306987946024 141.01767601670747 0 37.519288771127485 141.01763697594671 0 37.51914379710957 141.01774360080915 0 37.518969944519235 141.01787322707457 0 37.51888925152402 141.01793072557956 0 37.51875830698604 141.01802643194182 0 37.51856460547513 141.0181697521311 0 37.51843291360928 141.01826781011383 0 37.51828427359055 141.01837847414194 0 37.5181669260268 141.01846334654522 0 37.51806521551036 141.0185386617671 0 37.517945444776316 141.01862417747 0 37.51791122239609 141.01864328532358 0 37.517895781880625 141.01865004215787 0 37.517881730388126 141.0186539646431 0 37.517861528461374 141.0186577886972 0 37.51784316882651 141.01865832517043 0 37.517819807017 141.01865702460208 0 37.5177699983371 141.01865109074853 0 37.51757406439185 141.01862167352746 0 37.517510907009964 141.01861302066447 0 37.517489528785696 141.01860826963238 0 37.51743365336198 141.01859174338418 0 37.51726998956814 141.01852387816697 0 37.51725868731687 141.01856684021845 0 37.517423851385 141.01863532766114 0 37.51748224944641 141.0186525997213 0 37.51750580513094 141.01865783543366 0 37.517569970291156 141.01866662644733 0 37.517766156134854 141.01869608170549 0 37.51781730576823 141.01870217481112 0 37.51784278925769 141.01870359266923 0 37.517864646852736 141.01870295543836 0 37.51788833614415 141.01869847050787 0 37.517905656778396 141.01869363482191 0 37.51792449834094 141.01868539108395 0 37.51796171455713 141.01866461115867 0 37.518083278643005 141.01857781391996 0 37.51818507631692 141.01850243563308 0 37.51830246021613 141.01841753669694 0 37.5184512963513 141.0183067276321 0 37.518582946489154 141.0182087007094 0 37.518776522768285 141.01806547363066 0 37.518907213178586 141.01796995334348 0 37.518988042325574 141.01791235747183 0 37.519162107401556 141.0177825717288 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_0166ced2-d381-4164-ae77-a531e9ebca2a">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.519300838495674 141.01760708595748 0 37.51936164067508 141.01756523531304 0 37.51915021722324 141.0170810891414 0 37.51910416298902 141.01696531494633 0 37.51909170914999 141.0169312942914 0 37.51907603419058 141.01688602354483 0 37.51905329881856 141.01681699850093 0 37.51904869927434 141.01679952747074 0 37.51898047599153 141.01682783926964 0 37.51898672557418 141.01685158075543 0 37.5190097243958 141.01692128864946 0 37.51902557158716 141.01696735394708 0 37.51904205226092 141.01700415315577 0 37.5190887212163 141.017121520718 0 37.519300838495674 141.01760708595748 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_f6013945-4555-43de-8b9e-6cd2501527fc">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51933429572632 141.01768366680548 0 37.51939508614068 141.01764182393828 0 37.51936164067508 141.01756523531304 0 37.519300838495674 141.01760708595748 0 37.519288771127485 141.01763697594671 0 37.519306987946024 141.01767601670747 0 37.51933429572632 141.01768366680548 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_6ab3477f-74b2-4d2e-829e-7177c08ab5ab">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51941402432977 141.02393962179673 0 37.519486288798326 141.02392711384147 0 37.51947617302646 141.0238347638565 0 37.51945846488681 141.0236696609142 0 37.51945117767444 141.02360596949578 0 37.51944097680943 141.0235542229372 0 37.519426499484794 141.02349765725634 0 37.519404481942296 141.02343825685193 0 37.51937467105207 141.0233832576475 0 37.519342069681095 141.023336924695 0 37.51930956185427 141.02329919067057 0 37.51926706429878 141.02326027990892 0 37.51922225864184 141.0232268755912 0 37.519174628342256 141.0232055306707 0 37.51908844977481 141.02317361873918 0 37.51904069632655 141.02315555251306 0 37.51895746107942 141.02312662864279 0 37.51892309454619 141.02311239500207 0 37.518880790917194 141.0230900035991 0 37.51885741534117 141.02307594451972 0 37.51882569209815 141.0230497618597 0 37.51879250112835 141.0230172210327 0 37.518748598411 141.0229657317576 0 37.51872431369722 141.02292564005745 0 37.518699735827305 141.0228788694843 0 37.51868005819775 141.02283760663053 0 37.51865574363221 141.022782582335 0 37.51861496487353 141.0226803508074 0 37.51857448373932 141.02258434585815 0 37.51852782606695 141.02247534701357 0 37.51848430777193 141.02236775559496 0 37.51844153528064 141.02226662411277 0 37.51838975570349 141.02214725025823 0 37.51833700338069 141.02203498783763 0 37.518310706608645 141.02198886926192 0 37.51828519785965 141.02194502566323 0 37.5182518374499 141.0218934783588 0 37.518223056970434 141.02185241098113 0 37.51818784349978 141.02180592485925 0 37.51812048332379 141.0217305352874 0 37.51801219973773 141.02161196302745 0 37.51791659272453 141.0215051305397 0 37.51784696075224 141.02143162855836 0 37.517777367416066 141.02135428119945 0 37.51767803574969 141.02125044460377 0 37.5175733998342 141.02113645641998 0 37.517504551126116 141.02106568244452 0 37.51743388327964 141.02098764002812 0 37.517408764543795 141.02095873542322 0 37.517383753188035 141.0209370722726 0 37.51736133510137 141.0209173732924 0 37.51732338717916 141.02089188527003 0 37.51727978746723 141.0208639321651 0 37.51723630651431 141.02084208948799 0 37.51718617254701 141.02081878395188 0 37.51714060042927 141.02080776775313 0 37.51709573993167 141.0207976678093 0 37.51704611856003 141.02079507149037 0 37.51699797981599 141.0207973628426 0 37.51695314824356 141.0208023083683 0 37.516914948073115 141.02081086570647 0 37.51686204870012 141.02082959712598 0 37.516808659069355 141.0208522799487 0 37.51672365857786 141.02089131660244 0 37.516674995695084 141.02090988882617 0 37.51660750538336 141.02093698595644 0 37.5165613809357 141.02095401468296 0 37.51651871971284 141.02096713909782 0 37.516474829076195 141.02097707666758 0 37.51638808428614 141.0209924434298 0 37.51629948968878 141.02100359542936 0 37.516211494775376 141.02100887471977 0 37.51607780491125 141.02101535274602 0 37.51592471499027 141.02102457746307 0 37.5158639511284 141.02102791284543 0 37.51583059509077 141.02102964651314 0 37.515742064180344 141.02103446478574 0 37.51557338186061 141.02104412100505 0 37.515395875268204 141.02105318485206 0 37.515197628638845 141.02106384292856 0 37.51518404464238 141.02106445676853 0 37.51518650582172 141.02115024110972 0 37.51534913991114 141.02113991238178 0 37.51566827706647 141.02112412352528 0 37.51608086117043 141.0211056700351 0 37.51621463998914 141.02109930669909 0 37.51630488942876 141.02109383701386 0 37.51639673182131 141.021082284125 0 37.516486273959856 141.02106650932876 0 37.516533687211265 141.02105572269093 0 37.51658014473341 141.0210414141832 0 37.51662871072113 141.02102351924927 0 37.51669647245857 141.02099631337245 0 37.5167464923903 141.02097719711017 0 37.51683257967474 141.0209376121683 0 37.51688334533849 141.02091601901233 0 37.51693443496357 141.02089805080715 0 37.516959450160456 141.02089245190732 0 37.517002477661904 141.02088770408298 0 37.51704601954423 141.02088556617352 0 37.517087805361356 141.02088781212504 0 37.517127361916444 141.0208966968536 0 37.51716673146645 141.02090625735119 0 37.51721040737341 141.02092663256667 0 37.517250033777884 141.02094649113948 0 37.51728996366762 141.02097201057828 0 37.517323525600716 141.02099460108988 0 37.517363720291286 141.02102962692436 0 37.5173857211242 141.02105497536581 0 37.517457903391914 141.0211347386153 0 37.51752693110105 141.0212056285674 0 37.51763138904577 141.02131938771046 0 37.51773036256784 141.02142299240893 0 37.51779986690729 141.0215002252487 0 37.51786940988521 141.02157361271307 0 37.517964751119536 141.0216799885165 0 37.51807195014413 141.02179888295947 0 37.51813699298178 141.02187174709968 0 37.518169183211974 141.02191422599088 0 37.51819600901347 141.02195254742398 0 37.51822741815759 141.02200100946283 0 37.518251598307096 141.02204256952498 0 37.51827576933338 141.02208503444655 0 37.51832622823316 141.0221923961624 0 37.518377214038274 141.02231006051431 0 37.51841954857148 141.0224099406364 0 37.51846315464175 141.02251775962307 0 37.51851007571207 141.02262744131443 0 37.51855011775455 141.02272230800708 0 37.51859115877353 141.02282533548959 0 37.51861679390752 141.0228834350289 0 37.51863797456635 141.02292754979052 0 37.51866467127834 141.02297865264703 0 37.51869418582039 141.02302731158207 0 37.51874494452786 141.02308694144588 0 37.51878313265105 141.02312431279896 0 37.518821378878435 141.02315591584164 0 37.51885084522849 141.02317357847738 0 37.518897450362395 141.02319830069172 0 37.518936214193516 141.0232143010481 0 37.519020436987375 141.02324358006044 0 37.519067562071825 141.02326141011096 0 37.51915203460562 141.02329272942043 0 37.51919114396379 141.02331020597728 0 37.51922682767697 141.02333679115392 0 37.51926307476667 141.02337005959922 0 37.51928979397929 141.02340102738683 0 37.51931688726415 141.0234395804277 0 37.51934077039808 141.02348373869103 0 37.519358493208664 141.02353119283515 0 37.519370714820106 141.02357912522666 0 37.519379457108656 141.0236236086434 0 37.51938625279646 141.02368242786875 0 37.51940387425199 141.02384718990527 0 37.51941402432977 141.02393962179673 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_fb598aea-c9a1-491d-88d9-4021e54f22fb">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.51950372012138 141.02395386072723 0 37.51950411107398 141.02392813123114 0 37.519486288798326 141.02392711384147 0 37.51941402432977 141.02393962179673 0 37.51941921526662 141.02398689617027 0 37.519491420166055 141.02397439862568 0 37.51950372012138 141.02395386072723 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_3153b3a1-e304-4a90-8c99-4cadc9ade24c">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.5195299227025 141.02164947196968 0 37.51955457939292 141.02162479134836 0 37.51946137362884 141.02147802458424 0 37.519268573166954 141.02116934994012 0 37.519253456460575 141.0211466767279 0 37.519237203332374 141.02112975707584 0 37.519217698122254 141.0211121422016 0 37.51919324863652 141.02109670237587 0 37.51917292783589 141.02108743328867 0 37.51913020181358 141.02107206121758 0 37.51892849667682 141.0210089377939 0 37.51892086955787 141.0210473522899 0 37.51912204203211 141.02111030899258 0 37.51916318793852 141.02112511190924 0 37.51918073541938 141.0211331175665 0 37.519201304430105 141.02114610671137 0 37.51921790940753 141.02116110091714 0 37.519231118238245 141.02117485235732 0 37.51924406906687 141.02119427738702 0 37.51943663802879 141.02150258066789 0 37.5195299227025 141.02164947196968 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_d3e6423f-1dd1-45d0-a831-cffd6c3aa44c">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.519580918518336 141.02166626637043 0 37.519590180038826 141.0216489242733 0 37.51957939393629 141.02162068936317 0 37.51955457939292 141.02162479134836 0 37.5195299227025 141.02164947196968 0 37.51955626272033 141.02169094700062 0 37.519580918518336 141.02166626637043 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_0c1d2c27-b7a7-4344-8e87-747d94378a91">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.519590180038826 141.0216489242733 0 37.51979052173391 141.02144973351517 0 37.519775418754634 141.02142579104765 0 37.51957939393629 141.02162068936317 0 37.519590180038826 141.0216489242733 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_71a62970-6167-4b25-82df-ca0adb30455f">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52015966358426 141.01930750166284 0 37.520219660582576 141.0192638801035 0 37.52018853350336 141.0191961910894 0 37.520144774880386 141.01911223845534 0 37.52007599314415 141.0189808299404 0 37.51999468543453 141.01883205111824 0 37.51994817179995 141.0187496503827 0 37.51992051142682 141.0187055013588 0 37.51988729904575 141.0186525313079 0 37.51979587819537 141.01851348032065 0 37.51976460136359 141.01846695521672 0 37.51974143943452 141.0184254454854 0 37.519674972490286 141.01828623540626 0 37.51942335941853 141.0177065682419 0 37.51939508614068 141.01764182393828 0 37.51933429572632 141.01768366680548 0 37.51936248022407 141.0177483666809 0 37.519614621154474 141.01832928622196 0 37.51969543764783 141.01850059033777 0 37.51971556043861 141.01854253842393 0 37.51973532017343 141.0185757702102 0 37.51975912146069 141.01861031032107 0 37.51979115465811 141.01865957374721 0 37.51993579455305 141.0188907133625 0 37.520019175890674 141.0190308369597 0 37.52008680911829 141.0191599647419 0 37.52012932689387 141.01924186144655 0 37.52015966358426 141.01930750166284 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_b8c1f0a0-c4cd-4344-8429-1c972d31559f">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52025764581788 141.01933054648757 0 37.520274837527104 141.0193144154795 0 37.52025497052497 141.01927394154797 0 37.520246899132786 141.01927913066572 0 37.520227546829325 141.01927712745263 0 37.520219660582576 141.0192638801035 0 37.52015966358426 141.01930750166284 0 37.52017543869062 141.01934270702228 0 37.520194498616405 141.01939176565816 0 37.52025848040054 141.01935510786396 0 37.52025764581788 141.01933054648757 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_b9e5cf57-42fa-4568-b84d-86bd8f30e6dd">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52026760789078 141.01842078275473 0 37.520291877583574 141.0184068551782 0 37.52027977886059 141.01837340498636 0 37.520242034384225 141.01827371032013 0 37.52019997527467 141.01816399250998 0 37.52015262457922 141.01805181552075 0 37.52011861867756 141.01796564217102 0 37.520083896991785 141.0178879419524 0 37.520063764224524 141.01785594866453 0 37.52004106695248 141.01782810046953 0 37.51999110662808 141.01777844068835 0 37.51990378131981 141.01769900412415 0 37.51988415095034 141.01767980199034 0 37.519864577436806 141.01765762558833 0 37.51984299702135 141.0176280191272 0 37.51981866454406 141.01758621960664 0 37.51977817999746 141.01750991072174 0 37.51975327032125 141.01745557927768 0 37.519744925446986 141.01743233606788 0 37.51973957267743 141.0174108257049 0 37.51973809068653 141.01739327931082 0 37.51974028492229 141.01736944473546 0 37.51974712988896 141.017322108524 0 37.51975565055224 141.01725951567107 0 37.51975728482674 141.0172196311906 0 37.519757402617024 141.01718181554844 0 37.519756114702375 141.0171449053154 0 37.519740277843674 141.01694049943225 0 37.51971734463681 141.01667362737746 0 37.519702101278696 141.01652052165417 0 37.51969330808106 141.0164637640471 0 37.51968372287447 141.01642849888495 0 37.519678172577805 141.01641319601643 0 37.519667678762396 141.01638947782624 0 37.519634795789166 141.0163170115493 0 37.51957322655387 141.01617587853985 0 37.51954064030483 141.01609176534626 0 37.51951012946441 141.0160073456143 0 37.51948171163328 141.01592985955415 0 37.51944345786591 141.01581782861675 0 37.51942635689364 141.01577095164282 0 37.519413891385724 141.01573806209018 0 37.519385854247666 141.01567653270894 0 37.51935075254278 141.01560063828933 0 37.519305663241674 141.0155233419871 0 37.51928996072521 141.01549877261226 0 37.51927474892708 141.01547918843488 0 37.51924307271585 141.01545101745717 0 37.51920569073592 141.0154290031687 0 37.51914365340993 141.0153885442639 0 37.519101909162785 141.01536412974065 0 37.51907190671413 141.01534589602778 0 37.51905204821859 141.01533144230976 0 37.51904223339925 141.015321785106 0 37.51904079154204 141.01531937319345 0 37.519019167118785 141.01533974719788 0 37.51902271072348 141.01534579864037 0 37.519036899201126 141.01535959728676 0 37.51905917201245 141.01537589907358 0 37.51909024874492 141.0153948284848 0 37.51913172497954 141.0154190125456 0 37.51919304610283 141.0154590076742 0 37.51924119642839 141.01549155513933 0 37.51925727677939 141.01550538370546 0 37.51926973201209 141.0155213044289 0 37.51928419137891 141.01554404420588 0 37.51932813101005 141.0156191730035 0 37.51936243774637 141.0156934711288 0 37.51938994566012 141.0157538609056 0 37.51940188534583 141.0157852715404 0 37.519418721705165 141.01583157870593 0 37.51945689103336 141.01594304266 0 37.519486809803894 141.01602360674167 0 37.51951634323608 141.01610665353434 0 37.51954919294785 141.01619144961114 0 37.519611202418375 141.01633360765666 0 37.51964487806275 141.01640789642315 0 37.51965402006242 141.01642849139355 0 37.51965857165331 141.01644104087916 0 37.51966712881412 141.01647252388773 0 37.519675393921844 141.01652587148735 0 37.51969049567592 141.01667755181595 0 37.51971338934534 141.01694396273513 0 37.51972914237604 141.017147292539 0 37.51973037084612 141.01718249250965 0 37.519730258436894 141.01721869280664 0 37.51972873878193 141.01725577246748 0 37.519720509947966 141.01731621941553 0 37.519713534267495 141.01736446080258 0 37.51971089611384 141.01739311876486 0 37.5197129757276 141.01741774147752 0 37.51971961299785 141.01744441250656 0 37.519729178046816 141.01747105402052 0 37.5197551943128 141.01752780004935 0 37.51979649508939 141.01760564674038 0 37.51982205259988 141.0176495505045 0 37.519845509287954 141.01768173192616 0 37.519865712286894 141.01770349293116 0 37.519886680390364 141.01772407370945 0 37.51997567882074 141.01780738297765 0 37.520024122351266 141.0178555481635 0 37.52004459459528 141.01788064618404 0 37.520062332366 141.0179088684809 0 37.52009564436778 141.01798337889767 0 37.52012956357107 141.01806921148258 0 37.520176825295394 141.01818127391556 0 37.520218620929015 141.01829030878298 0 37.520256278706974 141.01838966267903 0 37.52026760789078 141.01842078275473 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_c499e5d9-39ab-4f25-ac3e-82cf96d93669">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52033204745208 141.01848034327458 0 37.520339552760916 141.01844177314192 0 37.52032587938713 141.01841259677667 0 37.52031728338194 141.0184161939401 0 37.520311778812356 141.01841701187126 0 37.520303780099226 141.01841496221846 0 37.52029688289661 141.01841089373517 0 37.520291877583574 141.0184068551782 0 37.52026760789078 141.01842078275473 0 37.520295299516064 141.0184968438784 0 37.52029750182966 141.01850230420393 0 37.52033204745208 141.01848034327458 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_9552e02d-28fa-4469-93d8-732355b0353a">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52050688920372 141.01913165793917 0 37.520515700983864 141.01913349429233 0 37.52049302562806 141.01907657262473 0 37.52048783522208 141.019090970545 0 37.52025497052497 141.01927394154797 0 37.520274837527104 141.0193144154795 0 37.52050688920372 141.01913165793917 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_6bc0badf-f506-4d64-82b4-38099a7bb6f9">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52051579305031 141.01904356091742 0 37.5205498976157 141.01902192352418 0 37.52050980207208 141.0189223086192 0 37.52047622229141 141.01883862997008 0 37.52046092473603 141.01880071711653 0 37.52038699261959 141.0186168497818 0 37.52036316967896 141.01855753457966 0 37.52033204745208 141.01848034327458 0 37.52029750182966 141.01850230420393 0 37.52051579305031 141.01904356091742 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_0a175015-429e-4502-9654-f5e47b1ee2cc">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52053265570102 141.0191230157138 0 37.52055087127628 141.0191305440615 0 37.52051579305031 141.01904356091742 0 37.52050998034039 141.0190660940362 0 37.52049302562806 141.01907657262473 0 37.520515700983864 141.01913349429233 0 37.52053265570102 141.0191230157138 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">3</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_8396864d-22d8-4a41-ae27-d5324dc4d411">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52055087127628 141.0191305440615 0 37.52058492458882 141.01910898733763 0 37.520577669678154 141.01909092173048 0 37.5205498976157 141.01902192352418 0 37.52051579305031 141.01904356091742 0 37.52055087127628 141.0191305440615 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_59cd154d-2641-4762-b4c1-a3d21ec6fc04">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.5206587516756 141.0193980677489 0 37.52069256743938 141.01937657296986 0 37.52066540212144 141.01930917244346 0 37.52064359935692 141.01925509256526 0 37.52058492458882 141.01910898733763 0 37.52055087127628 141.0191305440615 0 37.5206587516756 141.0193980677489 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_36f74f6a-2edb-4a3a-9150-c2f3151255f3">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.520713166824784 141.01942486444767 0 37.52072366087287 141.0194027447864 0 37.52071141555048 141.01937494821468 0 37.52069256743938 141.01937657296986 0 37.5206587516756 141.0193980677489 0 37.52067930611042 141.0194490395807 0 37.52071150771699 141.01942857133486 0 37.520713166824784 141.01942486444767 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_3ce553ae-ace9-42ff-ba61-be9debf82592">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.520922531989235 141.02324415087412 0 37.520944671709245 141.02322588760168 0 37.520937685912315 141.02320192286567 0 37.52091047693717 141.0232243885866 0 37.52088323008831 141.02325472613418 0 37.520839480941014 141.0233042592247 0 37.52080619810233 141.02334343779128 0 37.52075977105202 141.02339556068188 0 37.520720214308746 141.02343267450883 0 37.52068074553845 141.0234654388771 0 37.52065529853453 141.02348508504278 0 37.520631189025565 141.02350067313006 0 37.520605305757684 141.02351559613166 0 37.52056539587084 141.02353392536014 0 37.520488156283335 141.02357044375054 0 37.520420092404336 141.02360102169953 0 37.520392720853735 141.0236128801721 0 37.52036353758318 141.0236205988792 0 37.520330234323474 141.02362487307295 0 37.52030596541911 141.02362570020293 0 37.52026248572173 141.02362425939992 0 37.52020354593988 141.02361942131446 0 37.52016994636266 141.0236196601499 0 37.52012056741873 141.02362404206588 0 37.520085913533386 141.02362974278762 0 37.520056049153396 141.0236355761602 0 37.52001604375532 141.0236470200383 0 37.51998238859803 141.02366010329206 0 37.519953375458016 141.02367677733272 0 37.5199175989483 141.02370025921115 0 37.519838648746216 141.02375415641947 0 37.51975585794612 141.02380817575192 0 37.51970582483479 141.0238399978751 0 37.51966483599185 141.02386211620882 0 37.519635519436584 141.0238771551625 0 37.51959123408414 141.0238967300788 0 37.51956297862042 141.023908584455 0 37.51953596906202 141.02391840655108 0 37.51950411107398 141.02392813123114 0 37.51950372012138 141.02395386072723 0 37.519541070647165 141.02394245973787 0 37.51956887965743 141.02393234718156 0 37.51959766718911 141.0239202693641 0 37.51964256347935 141.02390042362043 0 37.51967249863484 141.02388506870665 0 37.51971422495207 141.02386255146084 0 37.519764907376725 141.02383031658172 0 37.51984795396144 141.0237761305138 0 37.51992692595432 141.02372221783114 0 37.51996209836569 141.02369913361122 0 37.519989474750275 141.02368339965767 0 37.52002118504392 141.02367107286187 0 37.520059778954504 141.02366003191983 0 37.520088724260255 141.02365437852956 0 37.52012255502257 141.0236488140664 0 37.52017070204054 141.02364454155676 0 37.520202955379325 141.0236443107564 0 37.52026157734797 141.02364912344947 0 37.520305973098644 141.02365059465856 0 37.52033151290787 141.02364972437843 0 37.52036659714058 141.02364522056575 0 37.52039801735224 141.02363691069343 0 37.520426664940075 141.02362450008752 0 37.52049498860234 141.02359380524038 0 37.52057229339188 141.02355725510046 0 37.520612857754955 141.02353862631585 0 37.52063986666679 141.02352305376144 0 37.52066504094617 141.02350677669006 0 37.520691406034224 141.02348642337134 0 37.520731622632674 141.02345303738446 0 37.520772338262724 141.02341483673158 0 37.52082007544746 141.0233611927478 0 37.52085290214408 141.023322572567 0 37.52089646883986 141.02327326283296 0 37.520922531989235 141.02324415087412 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_84440228-1723-4461-b294-ab8c07ac227b">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52104892782972 141.02036565491858 0 37.521081277401116 141.02034509315885 0 37.52105560666234 141.02028144878304 0 37.52098650454672 141.02011032519 0 37.52096567041476 141.02005852271643 0 37.52089279070039 141.01987772398235 0 37.52082913947784 141.01972003632184 0 37.52076979678391 141.01957293786143 0 37.52071150771699 141.01942857133486 0 37.52067930611042 141.0194490395807 0 37.52104892782972 141.02036565491858 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_3b83f286-7cea-46e6-ac83-5d1dabb44718">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52115189631421 141.02035543186923 0 37.521171562767925 141.0203288679962 0 37.521160823283665 141.02030681124901 0 37.52109966170139 141.02035375579945 0 37.521081277401116 141.02034509315885 0 37.52104892782972 141.02036565491858 0 37.5210538272181 141.02037780426187 0 37.5210644117539 141.02040048412096 0 37.521080717696925 141.0204276665734 0 37.52110364884164 141.02044563239886 0 37.52113189947609 141.02045878414978 0 37.521144779981455 141.02041081898216 0 37.52113177116861 141.02039356469447 0 37.52115189631421 141.02035543186923 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_329e657c-fc54-445c-a67e-abaa7eba32f1">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52122628102489 141.02188113201782 0 37.521304558397745 141.02183643649676 0 37.52129150367039 141.02180039763752 0 37.52121356162371 141.02184490166843 0 37.52117291169921 141.02186668638706 0 37.521142181188814 141.0218821304664 0 37.521118703219926 141.02189192919508 0 37.52108761804644 141.0219033097824 0 37.5210281023234 141.02192140020355 0 37.520980656361665 141.02193391038745 0 37.52092985950131 141.02194775545087 0 37.52083548671178 141.02197455213695 0 37.52066199149053 141.0220241835305 0 37.52050004109319 141.02206980771618 0 37.520398590579916 141.0220989319199 0 37.52034575488872 141.02211006674798 0 37.52031963945583 141.02211396696387 0 37.52029805191984 141.02211453961044 0 37.52027551014725 141.02211207214833 0 37.52025592701688 141.022108210419 0 37.52023615611074 141.02210214655994 0 37.52018445275475 141.02207902583768 0 37.520059795283096 141.0220159209149 0 37.51991516837652 141.02194239057573 0 37.519760800244526 141.02186411540703 0 37.51972889804146 141.0218462877197 0 37.51970283588554 141.0218261827351 0 37.51967670165622 141.02180374852375 0 37.519661232820866 141.02178688971634 0 37.51964760715578 141.02176836555194 0 37.519593527066114 141.02168611941312 0 37.519580918518336 141.02166626637043 0 37.51955626272033 141.02169094700062 0 37.51956903800386 141.02171106400215 0 37.519623846684745 141.02179441786228 0 37.5196392474459 141.0218153548422 0 37.519657403449244 141.021835142077 0 37.51968567272148 141.02185940883888 0 37.51971416860881 141.02188139184025 0 37.519748481064774 141.02190056549327 0 37.51990335835501 141.02197909993785 0 37.52004798165717 141.02205262801573 0 37.52017326125808 141.022116048298 0 37.52022710037803 141.02214012376993 0 37.5202497277824 141.0221470633028 0 37.52027168943878 141.02215139492208 0 37.52029701236835 141.02215416671484 0 37.520321840925014 141.02215350749498 0 37.52035023040378 141.02214926780098 0 37.520404720313294 141.02213778480368 0 37.52050700761776 141.02210842052702 0 37.52066894628261 141.02206279962883 0 37.52084246772421 141.0220131608179 0 37.52093668238618 141.02198640918246 0 37.52098724161978 141.02197262938103 0 37.521035054397395 141.02196002096431 0 37.521095746863985 141.02194157479354 0 37.521128093655506 141.021929731187 0 37.5211530238992 141.0219193265263 0 37.521184962519676 141.02190327490575 0 37.52122628102489 141.02188113201782 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_45ff218f-3adb-4e1c-8419-a2fc0cdaad53">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.521303079834304 141.02179086029807 0 37.52136015164594 141.02173823518194 0 37.52132042156429 141.02167031601806 0 37.521248611939484 141.02155356153546 0 37.52113576858867 141.02135979615431 0 37.52102020514573 141.0211676851257 0 37.520890974149516 141.02095436181875 0 37.52077918806935 141.02076556994362 0 37.52072888803002 141.0206861954736 0 37.52070006173411 141.02063978690543 0 37.52066273699847 141.02057023391248 0 37.52061623799297 141.02047753715655 0 37.520566558422686 141.0203763056988 0 37.52053167977321 141.02029645203683 0 37.52051137460542 141.02023685254719 0 37.5204908457432 141.02017261139946 0 37.52046530502611 141.0200780863904 0 37.52044010155812 141.0199768923853 0 37.52038694556819 141.0197611122488 0 37.5203521103051 141.0196320502346 0 37.52032393040924 141.019540085677 0 37.5202946901528 141.01945500504021 0 37.52025848040054 141.01935510786396 0 37.520194498616405 141.01939176565816 0 37.52023044715791 141.01949075362896 0 37.52025890043955 141.0195734461164 0 37.520286032182426 141.01966211337722 0 37.52032008501426 141.0197883347677 0 37.520372980917294 141.02000309248135 0 37.52039844326766 141.02010542175842 0 37.520424941987685 141.02020324250537 0 37.52044642999245 141.02027066631257 0 37.52046839796666 141.02033515649993 0 37.520505479949634 141.0204199094211 0 37.52059625060423 141.02061354820165 0 37.52063657423277 141.02068952897267 0 37.520680952993786 141.02076546097175 0 37.52074262261358 141.02086021977854 0 37.5208610467073 141.0210546374873 0 37.52096443052565 141.02122144078388 0 37.521079374633416 141.0214124106634 0 37.521191863231216 141.02160560471498 0 37.521264125531694 141.02172214008428 0 37.52130150602534 141.02178608400902 0 37.52130177174175 141.02178654073117 0 37.521303079834304 141.02179086029807 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_8e020c7c-6996-490a-ab08-d9651734c338">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.521327904911615 141.0218298507053 0 37.52138366618344 141.02177843337017 0 37.52136015164594 141.02173823518194 0 37.521303079834304 141.02179086029807 0 37.52129150367039 141.02180039763752 0 37.521304558397745 141.02183643649676 0 37.521327904911615 141.0218298507053 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_8c164caa-5ab7-4abe-8aa4-e43164b520a4">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.521171562767925 141.0203288679962 0 37.521428714186946 141.02013045301916 0 37.52141865877372 141.02010891273713 0 37.521160823283665 141.02030681124901 0 37.521171562767925 141.0203288679962 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_5ecf115c-e74e-4841-a683-23ec99f607e2">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52153338487894 141.02049475026047 0 37.521534017738 141.02045276977265 0 37.52151783900224 141.02045201451654 0 37.52125150569018 141.0204415723047 0 37.5212301691633 141.0204397635698 0 37.52117330787861 141.02042507244855 0 37.521144779981455 141.02041081898216 0 37.52113189947609 141.02045878414978 0 37.52117820676775 141.0204728555436 0 37.521227407263325 141.02048146353454 0 37.521249914958155 141.02048329083968 0 37.52151678766641 141.020493854874 0 37.52153338487894 141.02049475026047 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_b1f52c9f-4208-47e6-877d-f8ca85d635f1">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52161023314014 141.02240967418075 0 37.52167041939364 141.0223652654189 0 37.52158538457446 141.02218393048824 0 37.521559729787576 141.02212775170304 0 37.52154544607428 141.02209664134807 0 37.521502849214194 141.02200489816732 0 37.52145621872881 141.02191105481074 0 37.52139429760804 141.02179660616332 0 37.52138366618344 141.02177843337017 0 37.521327904911615 141.0218298507053 0 37.521336583989175 141.02184603203202 0 37.52139771019326 141.0219588842419 0 37.52144301434226 141.02205021771556 0 37.52148508429299 141.0221405949622 0 37.521525021623155 141.02222799716267 0 37.521558012609624 141.02229832014544 0 37.52161023314014 141.02240967418075 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_bbb9bbee-07bf-41d9-82f3-8e8efabf7cd0">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52168801033575 141.02467428045827 0 37.52169116900676 141.02460652558386 0 37.521589515819834 141.02459905369906 0 37.521550952678254 141.0245985513623 0 37.521532476010925 141.02459988093412 0 37.52148429262418 141.02460811401153 0 37.521404156697294 141.02462436422988 0 37.52139110643023 141.0246261975196 0 37.52137199598925 141.02462702490328 0 37.52136785756502 141.02462639346217 0 37.52135807086976 141.0246229571624 0 37.52135359490668 141.02462005781868 0 37.52134503871048 141.02461075849504 0 37.521339740578114 141.02460004033256 0 37.521302214709095 141.02449706396231 0 37.521216850415435 141.0242325742774 0 37.521168806829 141.02405833107176 0 37.521117722727155 141.02408053359437 0 37.52116589261271 141.02425529455869 0 37.52125256599159 141.02452399058708 0 37.52129193237764 141.0246320868695 0 37.52130641157128 141.02466161692854 0 37.52131958282655 141.02467789037243 0 37.521331483749535 141.02468611162172 0 37.52133983699416 141.02468873324588 0 37.52138439377271 141.0246931749871 0 37.52141071824155 141.0246917835793 0 37.52149224913276 141.0246752513034 0 37.521537676891064 141.0246674891763 0 37.521552221264 141.02466644212683 0 37.52158765459502 141.02466690377807 0 37.52168801033575 141.02467428045827 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_f470e8f8-7cdc-4787-bc03-8ecee6dc6512">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52175172919648 141.02243330109712 0 37.52176308592233 141.022424711718 0 37.52174214901456 141.0223810708415 0 37.52173591242567 141.02238353419136 0 37.5217268928623 141.02238533682592 0 37.52171219213714 141.02238491118305 0 37.52169615792109 141.02238090086496 0 37.52168171175577 141.0223739857565 0 37.52167041939364 141.0223652654189 0 37.52161023314014 141.02240967418075 0 37.52167978106174 141.02257198444713 0 37.52174084890906 141.02252951293644 0 37.5217348377325 141.02250905458845 0 37.52173405973548 141.02248788741912 0 37.521736429184024 141.02246733584604 0 37.52174053778378 141.02245303388287 0 37.52175172919648 141.02243330109712 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_17ad4741-7e9a-4552-a14c-d5ee679d6c2e">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52175659151675 141.02460611447077 0 37.52179367726824 141.02459774223166 0 37.521786087189504 141.0245447435134 0 37.52177999741762 141.02449057403473 0 37.52174431272655 141.0244968983716 0 37.52175659151675 141.02460611447077 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_35b660ff-4b8f-41d2-ada8-ae47f96e3cfc">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.521754055404394 141.02475381058994 0 37.521808395305236 141.02470050595537 0 37.52179367726824 141.02459774223166 0 37.52175659151675 141.02460611447077 0 37.52174837269123 141.02461294450725 0 37.52172228516057 141.0246163047354 0 37.5217099525768 141.02461246592856 0 37.52169116900676 141.02460652558386 0 37.52168801033575 141.02467428045827 0 37.52169509969881 141.02467714218884 0 37.52170613325947 141.02468381115617 0 37.52171699357264 141.02469425582328 0 37.521754055404394 141.02475381058994 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_7da75d8a-8d6a-48ba-8138-39177b93a6f0">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52186965981741 141.02493957980286 0 37.52190790217853 141.02490543528106 0 37.52183805297883 141.02478688210104 0 37.52183451688627 141.0247692390613 0 37.52181487592887 141.02472172991494 0 37.521808395305236 141.02470050595537 0 37.521754055404394 141.02475381058994 0 37.52186965981741 141.02493957980286 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_eee026bf-9177-4b25-b2d8-07ce8111edc2">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52195157032598 141.02228216688007 0 37.52196799970095 141.02227914691852 0 37.52192253115468 141.02223996190492 0 37.52191340374063 141.02225158232935 0 37.52174214901456 141.0223810708415 0 37.52176308592233 141.022424711718 0 37.52195157032598 141.02228216688007 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_04184992-b5c1-43a5-9e8c-e17d057cdc4c">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.521948542552856 141.0221980649424 0 37.521979009866605 141.0221879144318 0 37.5219663708328 141.0221724416156 0 37.521929791539456 141.02213633904609 0 37.52192748319766 141.0221332830384 0 37.52190947161843 141.02215867719752 0 37.52191160746262 141.02216150421262 0 37.521948542552856 141.0221980649424 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_45df4506-316e-4bd4-a8a1-1510d8f08580">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52196799970095 141.02227914691852 0 37.5219984246326 141.0222553073045 0 37.52195331871548 141.02221590178817 0 37.52192253115468 141.02223996190492 0 37.52196799970095 141.02227914691852 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">3</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_98b6fd97-06f0-4a66-9eb0-9861aca04d7e">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52204279228232 141.02224578806445 0 37.52204396002367 141.02221625420836 0 37.5219894549616 141.0221949640126 0 37.521979009866605 141.0221879144318 0 37.521948542552856 141.0221980649424 0 37.52195331871548 141.02221590178817 0 37.5219984246326 141.0222553073045 0 37.52204279228232 141.02224578806445 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_5bc612e9-b8e6-4611-b124-172e789af19b">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52203804429072 141.02509159815304 0 37.52204383794011 141.02503535266476 0 37.521944763344415 141.02490905339914 0 37.52190790217853 141.02490543528106 0 37.52186965981741 141.02493957980286 0 37.5218747307564 141.02497638166864 0 37.52189821248198 141.0250321322712 0 37.52203804429072 141.02509159815304 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_c430adf4-6af2-46a3-90db-b1d79b9ff2de">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.5220816481844 141.02482882829355 0 37.52212156120181 141.02463354812727 0 37.522050637294434 141.0246156128941 0 37.522046939362795 141.02463866271222 0 37.52203553865601 141.02469696849622 0 37.52202232135214 141.0247477789376 0 37.522009886304424 141.0248014299963 0 37.52199842775605 141.02483777670903 0 37.5219864866976 141.02486032546528 0 37.521971053005814 141.02488253584386 0 37.521944763344415 141.02490905339914 0 37.52204383794011 141.02503535266476 0 37.522063393002895 141.02491902892046 0 37.522069767982316 141.02488553134503 0 37.5220816481844 141.02482882829355 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_492bb480-9da5-4ece-8b5a-0a7ed7f1645f">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.522187588575484 141.017067539906 0 37.5222013865651 141.01705735013934 0 37.52218900979083 141.01703362392539 0 37.52217466672804 141.01704425759473 0 37.52216240480472 141.01705401911914 0 37.52160665315547 141.01746629749857 0 37.52070848001696 141.01813244031635 0 37.52039143213625 141.01837200354117 0 37.520352386564085 141.0183929928809 0 37.52032587938713 141.01841259677667 0 37.520339552760916 141.01844177314192 0 37.52040264202152 141.01839525843656 0 37.52057814452047 141.01826262306415 0 37.52060365727893 141.01824334270103 0 37.52070416426964 141.01816744108496 0 37.52071959872296 141.01815580696007 0 37.520932304506935 141.01799717411936 0 37.52160634579208 141.01749695012228 0 37.522161373422996 141.01708499991702 0 37.52218414588076 141.0170694086859 0 37.522187588575484 141.017067539906 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_5597a55c-a01b-4ca9-81a5-69ba10093e55">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.522063702482065 141.0245043071329 0 37.522133546006124 141.02450304197725 0 37.52213050439868 141.02425965537782 0 37.522126464399584 141.0240794914513 0 37.52212496488846 141.0239601176265 0 37.52212117310195 141.0238446668558 0 37.5221092411293 141.02369650590398 0 37.52209346483481 141.02357215390978 0 37.522075173946405 141.02346484432985 0 37.52205135438287 141.02335179058628 0 37.52202591998424 141.02324696959496 0 37.52199105221163 141.02313045982137 0 37.521955407837794 141.02302841816942 0 37.52191654642354 141.02293266067875 0 37.521833349522254 141.0227389096621 0 37.52174084890906 141.02252951293644 0 37.52167978106174 141.02257198444713 0 37.521772105979984 141.0227809257524 0 37.52185468585977 141.0229733093258 0 37.52189231675265 141.0230659927698 0 37.52192629497458 141.02316348278364 0 37.52195976914875 141.02327521897107 0 37.52198416576028 141.02337572453868 0 37.52200729878272 141.0234853734473 0 37.52202482444787 141.02358814566412 0 37.522039926755774 141.02370784859679 0 37.52205145958106 141.02385091231676 0 37.52205510881706 141.02396262749554 0 37.52205670411991 141.0240814370923 0 37.52206065402919 141.0242615994188 0 37.522063702482065 141.0245043071329 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_2e87994a-55f8-4e1a-8a85-3f4d4a8ee995">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52220353477676 141.01701382974667 0 37.52225010354012 141.0169793826732 0 37.521828349121094 141.01608670382657 0 37.52180920400338 141.01604601502964 0 37.52162480182965 141.01565496414082 0 37.52139045897068 141.01515803236185 0 37.521372811097386 141.0151207614654 0 37.52116926130072 141.0146889138596 0 37.520954596047474 141.0142331368674 0 37.52093694787356 141.01419586640066 0 37.520926008555705 141.01417250309493 0 37.520518815755096 141.01330825342873 0 37.52049958521203 141.0132669990786 0 37.52008241431139 141.0123816740919 0 37.52006344467189 141.01234132932498 0 37.519666650938284 141.01149909607625 0 37.51964565179094 141.011454421118 0 37.51962721446836 141.0114148770611 0 37.51946258644212 141.0110633006977 0 37.51943576960918 141.01101468831973 0 37.51941008707307 141.01108724959911 0 37.51941699003076 141.01109980175082 0 37.51962008371599 141.0115335454545 0 37.52001881741447 141.01237999450336 0 37.52003487617607 141.01241407157434 0 37.520453455511664 141.01330269899287 0 37.52047127737577 141.01334065049767 0 37.520889850662705 141.01422917504055 0 37.52091146691879 141.0142751056328 0 37.521122692049886 141.01472347500464 0 37.5213266853834 141.01515600818885 0 37.52134424315588 141.01519327764888 0 37.52162349606196 141.01578537929197 0 37.52176492913112 141.0160853631884 0 37.521782398777276 141.01612240543756 0 37.52220353477676 141.01701382974667 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_ff2579c5-c45b-4899-acf9-7c0a2dd55980">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52213652296843 141.0246098821107 0 37.522175183159796 141.02459897704765 0 37.52216375864822 141.02453513033197 0 37.52215057410287 141.02452821219867 0 37.52213482205157 141.0245105287123 0 37.522133546006124 141.02450304197725 0 37.522063702482065 141.0245043071329 0 37.52206174652663 141.02454635959376 0 37.522050637294434 141.0246156128941 0 37.52212156120181 141.02463354812727 0 37.52213652296843 141.0246098821107 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_c51f8df1-6c97-4bad-ad7e-9bc4c515ca57">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.522222769641836 141.01705452042242 0 37.52226924715451 141.01701991920095 0 37.52225010354012 141.0169793826732 0 37.52220353477676 141.01701382974667 0 37.52219510817816 141.01702749823608 0 37.52218900979083 141.01703362392539 0 37.5222013865651 141.01705735013934 0 37.52221042277633 141.01705477784387 0 37.522222769641836 141.01705452042242 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_8de34fe3-0a86-4991-a598-2c6e1864e439">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52232201671267 141.02455755733038 0 37.522331900947044 141.02455614767436 0 37.52232772294491 141.02450995319396 0 37.52230102087605 141.02451662175753 0 37.52216375864822 141.02453513033197 0 37.522175183159796 141.02459897704765 0 37.52232201671267 141.02455755733038 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_5eb712bf-b8f2-4d9c-b53c-c902e659284a">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.5223427302648 141.02441744925576 0 37.522366276776566 141.02441454290684 0 37.522361940159726 141.02435315841544 0 37.522359660660435 141.02432008871014 0 37.522358299186806 141.02427639954064 0 37.522355410710446 141.0242411707205 0 37.52229904192232 141.02333479637008 0 37.52229602417611 141.02327660052916 0 37.52229353217743 141.0232288208391 0 37.522291417670026 141.02318828735326 0 37.52229379561631 141.0230595852098 0 37.52229423667984 141.02302474872283 0 37.52228829503046 141.02298845333826 0 37.52224594922814 141.02283607603542 0 37.52216281175177 141.02256483250795 0 37.52211088921028 141.02237915720133 0 37.52208565739831 141.02230782552402 0 37.52206964153301 141.0222787237259 0 37.522047369604984 141.02225348213054 0 37.52204279228232 141.02224578806445 0 37.5219984246326 141.0222553073045 0 37.52202927559319 141.02227864873646 0 37.52204914128302 141.02230125020318 0 37.52206197198946 141.02232453192397 0 37.52208571326497 141.02239176732124 0 37.52213746472073 141.02257653483366 0 37.522220602171906 141.0228477782808 0 37.522262163359905 141.0229975411303 0 37.52226717327356 141.0230279390409 0 37.522266770953735 141.02305892977256 0 37.52226428924319 141.0231889877596 0 37.522272076578055 141.0233371962366 0 37.52232843965019 141.0242441358121 0 37.5223427302648 141.02441744925576 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_860ec66c-4645-4060-81c1-2dde9e664ea3">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.522350638936274 141.02455347601153 0 37.5223771616729 141.02455082971153 0 37.522374227079844 141.02450445926218 0 37.522368142346984 141.0244350487932 0 37.522366276776566 141.02441454290684 0 37.5223427302648 141.02441744925576 0 37.5223444794266 141.02445842950362 0 37.52234073417694 141.02448849601436 0 37.52232772294491 141.02450995319396 0 37.522331900947044 141.02455614767436 0 37.522350638936274 141.02455347601153 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_0934b3c2-e2ef-4d6e-9a0e-6debcda5caea">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52249045905608 141.0245725893943 0 37.52249424719741 141.0244989690354 0 37.52244904093946 141.02450174590604 0 37.522386820773434 141.02450447623949 0 37.522374227079844 141.02450445926218 0 37.5223771616729 141.02455082971153 0 37.52238741143005 141.02454980710849 0 37.52240545765831 141.02455086333543 0 37.522425271005396 141.02455359933683 0 37.52244605880495 141.02455620376773 0 37.52249045905608 141.0245725893943 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_31fa2be1-757c-42d4-9ff1-74fa39be42c8">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52262702999559 141.01799274992086 0 37.52263447410755 141.01798721119997 0 37.522624341841016 141.01796583819674 0 37.52159391643717 141.018735836033 0 37.521152246044736 141.01906585153435 0 37.52102291044102 141.01915917003691 0 37.52097327800668 141.01919356671002 0 37.52071141555048 141.01937494821468 0 37.52072366087287 141.0194027447864 0 37.521129614099344 141.01912126470177 0 37.521165918112 141.019095141449 0 37.52134876067309 141.01895843689198 0 37.52134696076648 141.01894924513365 0 37.52159362464014 141.01876490510804 0 37.52262702999559 141.01799274992086 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_59e0e290-3069-4a12-88a9-3078b3d31752">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.522640594566724 141.0179429614979 0 37.52268710352606 141.01790848445245 0 37.52227039682152 141.01702235265427 0 37.52226924715451 141.01701991920095 0 37.522222769641836 141.01705452042242 0 37.522640594566724 141.0179429614979 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_cadd5957-3654-4e3b-ba6e-b5b565db3387">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52265867835693 141.0179815981279 0 37.52270526617063 141.0179470759626 0 37.52270374876798 141.01794384702632 0 37.5226878684264 141.01791010976513 0 37.52268710352606 141.01790848445245 0 37.522640594566724 141.0179429614979 0 37.52263325688385 141.0179558552777 0 37.522627231519394 141.01796367901215 0 37.522624341841016 141.01796583819674 0 37.52263447410755 141.01798721119997 0 37.52264514443769 141.0179834204278 0 37.52265867835693 141.0179815981279 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_3be77414-ca1d-4cf7-a07b-e19557781e0b">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52392647063445 141.0157347725973 0 37.52395203675078 141.01571579117595 0 37.52393165744999 141.01567236668564 0 37.52390655596147 141.01561461321705 0 37.523766698044724 141.01530118382857 0 37.5234166542961 141.01455262291623 0 37.523023470466676 141.01372046533066 0 37.52266526934522 141.01295764965815 0 37.522650832392195 141.01292359679707 0 37.52224263551208 141.01205072011564 0 37.52216964290187 141.01190121991758 0 37.522041673402846 141.0116241700126 0 37.52189171943691 141.011308023453 0 37.521768675680285 141.01104215196517 0 37.52162393308846 141.01073662743465 0 37.521270288342286 141.01002097257316 0 37.521248297657756 141.00997628148158 0 37.52121426070406 141.00990800662 0 37.52119050489127 141.00992743070915 0 37.52123214845794 141.01001451356703 0 37.52128714783641 141.01012757660112 0 37.52132418543749 141.01020938349026 0 37.52159533363447 141.0107583509107 0 37.52174876067272 141.01106222447436 0 37.521872373738546 141.01132250618483 0 37.52202225441582 141.01163849881485 0 37.522149992813205 141.0119150472717 0 37.52221218480641 141.01205929125942 0 37.52262505698003 141.01294219658163 0 37.52264536643303 141.01298369528232 0 37.52264900137328 141.0129896352217 0 37.52300023102402 141.01373774733543 0 37.523063143477536 141.01387098701161 0 37.52308369685602 141.01391509202938 0 37.52308520237905 141.0139177177344 0 37.52339341473603 141.014569904747 0 37.52350174488603 141.01480160650044 0 37.52352141850852 141.01484354868273 0 37.523743282727615 141.01531801020846 0 37.523882962686244 141.01563121045862 0 37.523905615881056 141.01569050910163 0 37.52392290884605 141.01572721015145 0 37.52392647063445 141.0157347725973 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_ad6d25c1-d7e7-482b-8c1c-5477e027f837">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52394205177641 141.015767857007 0 37.52397228394062 141.01574541164308 0 37.52395203675078 141.01571579117595 0 37.52392647063445 141.0157347725973 0 37.52394205177641 141.015767857007 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_da651abc-8938-4eea-84b9-4d340f3bdb0e">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52397228394062 141.01574541164308 0 37.52397782020456 141.0157414263194 0 37.523961567553364 141.01570881436862 0 37.52395203675078 141.01571579117595 0 37.52397228394062 141.01574541164308 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_3a742640-9bcd-4264-9e18-a426ec313a71">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52204396002367 141.02221625420836 0 37.52395925567779 141.02079372387726 0 37.52393769523569 141.02074796464422 0 37.5219894549616 141.0221949640126 0 37.52204396002367 141.02221625420836 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_33ef8924-0a98-41bc-93f2-454b0ab69939">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52393777979944 141.02069286679532 0 37.52398426178194 141.02065819361619 0 37.523611127708705 141.01987054954714 0 37.52358219079359 141.01980911403044 0 37.523562870226215 141.01976808116163 0 37.523143189146104 141.01887744013678 0 37.52312351469871 141.0188357233711 0 37.52293815261695 141.01844249412284 0 37.52270526617063 141.0179470759626 0 37.52265867835693 141.0179815981279 0 37.522891582307025 141.0184770536913 0 37.5230795023793 141.0188757534715 0 37.52309714789712 141.01891313919225 0 37.52351709330451 141.01980434969659 0 37.52353526769399 141.01984287553216 0 37.52356464708578 141.01990510994264 0 37.52393777979944 141.02069286679532 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_db02b011-06ec-46c5-9f4a-62d78f86bc9c">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52397782020456 141.0157414263194 0 37.52407430958994 141.01567065864404 0 37.52405805691991 141.01563804666552 0 37.523961567553364 141.01570881436862 0 37.52397782020456 141.0157414263194 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">3</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_99518aba-5c4a-4d32-8cc0-6dd0dc4afe09">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.524085111842766 141.01566268370868 0 37.52408970427599 141.01565928193935 0 37.52407430535835 141.01562617321036 0 37.52406949433064 141.01562962922785 0 37.52405805691991 141.01563804666552 0 37.52407430958994 141.01567065864404 0 37.524085111842766 141.01566268370868 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_87c17c10-08f1-46ee-8a1f-bacf4b7e782e">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52404478831206 141.02073311793845 0 37.524053647317984 141.02072398162895 0 37.52403075054379 141.02067527108653 0 37.52401844637345 141.02068158787407 0 37.52401071001884 141.0206803339419 0 37.52400237146783 141.0206762421944 0 37.52398426178194 141.02065819361619 0 37.52393777979944 141.02069286679532 0 37.52394069488867 141.02070763130897 0 37.52394157679461 141.02072656074628 0 37.52393769523569 141.02074796464422 0 37.52395925567779 141.02079372387726 0 37.52396786076088 141.02079460694642 0 37.52398054628169 141.02080189002248 0 37.523994606834265 141.02081221549082 0 37.524005727023244 141.02082641999155 0 37.52404449226038 141.02079750734714 0 37.52403999100155 141.02077292053175 0 37.524039585333256 141.0207586030449 0 37.52404245974869 141.02074067211072 0 37.52404478831206 141.02073311793845 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_8c72fc8e-f3a7-4f57-b7da-faa0d1ca9254">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52407430535835 141.01562617321036 0 37.52412198050796 141.01559097487325 0 37.52408893295452 141.01552041389485 0 37.52406823099917 141.015482210936 0 37.52401099575112 141.01538104885074 0 37.52396723368582 141.01528871102127 0 37.523845886291184 141.0150300825698 0 37.52364631780318 141.0146034841461 0 37.52351420752783 141.01432136079146 0 37.52338074547324 141.01403666717613 0 37.523198671302204 141.0136466993736 0 37.52307141880561 141.01337396646719 0 37.52290058784944 141.01300831052748 0 37.522693293588496 141.01256637522664 0 37.5225389279033 141.01223886435054 0 37.522414579578076 141.0119750885334 0 37.52224400648954 141.01161484491516 0 37.52204516742949 141.01119200146994 0 37.5218598629347 141.01080397761706 0 37.52167705294895 141.01042907014366 0 37.52153030843211 141.0101225518693 0 37.521409400459184 141.0098655127659 0 37.52137061143027 141.0098942736297 0 37.52149160846289 141.01015150183403 0 37.52163853738502 141.01045840527178 0 37.521821334026704 141.01083328639135 0 37.52200645308628 141.01122092143171 0 37.52220524970605 141.01164367356114 0 37.522375835051456 141.01200394439894 0 37.52250015949832 141.0122676688414 0 37.52265450038955 141.01259512830597 0 37.52286175132914 141.01303697000526 0 37.52303254955593 141.01340255629282 0 37.52315980815952 141.01367530277548 0 37.52334190253048 141.0140653137493 0 37.523475374224795 141.0143500277808 0 37.523607474727896 141.01463213051957 0 37.52380705812585 141.01505875957483 0 37.52392846984442 141.01531752697076 0 37.52405107996915 141.01557623492664 0 37.52407430535835 141.01562617321036 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_1ff3f7a1-ef30-474f-8993-78bf7a057c58">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52408970427599 141.01565928193935 0 37.52413745842494 141.01562402377246 0 37.52412198050796 141.01559097487325 0 37.52407430535835 141.01562617321036 0 37.52408970427599 141.01565928193935 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">4</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_c35e53ea-a5a1-4066-83b2-58562bb6ee09">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52528496145037 141.02354258939448 0 37.525327853318366 141.02351143082097 0 37.525308512777556 141.0234787675499 0 37.525113271442905 141.02306601279173 0 37.52510189260018 141.02304173484964 0 37.52509448232338 141.02302600487803 0 37.5248913055718 141.02259581727157 0 37.524872428476684 141.02255558193326 0 37.524712481820146 141.02221670157564 0 37.52446186617606 141.02168315544256 0 37.52443437394564 141.02162151518323 0 37.52404449226038 141.02079750734714 0 37.524005727023244 141.02082641999155 0 37.52439534507399 141.02164974457327 0 37.52442647617031 141.02170793546327 0 37.524711497859386 141.022314431903 0 37.52483836002382 141.02258332408516 0 37.524863856000486 141.02263735348293 0 37.52506173661975 141.0230565962129 0 37.52506950041387 141.02307301058985 0 37.5250784103004 141.02309193208708 0 37.52527374163746 141.02350468814723 0 37.52528496145037 141.02354258939448 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_190dc553-b111-461d-8796-833bf0029589">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">3</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.525672434927834 141.0195232424306 0 37.525698635704856 141.01950428777204 0 37.525660639785784 141.01946588257863 0 37.525649456871285 141.01947459211956 0 37.52531201013193 141.01972584744527 0 37.52403075054379 141.02067527108653 0 37.524053647317984 141.02072398162895 0 37.52533494813091 141.01977452865293 0 37.525672434927834 141.0195232424306 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_1c201bb9-28e1-4bee-9ba4-7f09dd068ce8">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.525661347558035 141.01942322667654 0 37.52570656661705 141.01943611811527 0 37.525688666842036 141.0194035215205 0 37.525441502782286 141.01886595945655 0 37.52523313731984 141.01842155727095 0 37.525039521250974 141.01800669211156 0 37.524777629854206 141.0174545387305 0 37.52476077201344 141.01741942758963 0 37.5244232611052 141.01670736005454 0 37.524163917108574 141.016143150947 0 37.524014087828185 141.01582684695174 0 37.52397228394062 141.01574541164308 0 37.52394205177641 141.015767857007 0 37.52395484753915 141.01579502710544 0 37.523983483969104 141.0158506869871 0 37.52413278284464 141.01616596434366 0 37.524362850490675 141.01666647033096 0 37.52438487823949 141.01670720622772 0 37.52473253740376 141.0174405894848 0 37.52476001547568 141.01749487467768 0 37.524806958734764 141.0175938154076 0 37.524830165734556 141.0176427159746 0 37.52501237896684 141.0180267398119 0 37.52502957768962 141.01806377984929 0 37.525033105253 141.01807141551257 0 37.525205904877915 141.01844160344368 0 37.52524489703249 141.0185246941003 0 37.525265891123794 141.01856960058015 0 37.525414271403285 141.01888589240437 0 37.52546259243544 141.0189910789949 0 37.52546867802853 141.01900418562295 0 37.525661347558035 141.01942322667654 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
	<core:cityObjectMember>
		<tran:Road gml:id="tran_e39a7538-0226-4dbc-8cae-d3f314016655">
			<core:creationDate>2023-03-24</core:creationDate>
			<tran:function codeSpace="../../codelists/Road_function.xml">9020</tran:function>
			<tran:lod1MultiSurface>
				<gml:MultiSurface>
					<gml:surfaceMember>
						<gml:Polygon>
							<gml:exterior>
								<gml:LinearRing>
									<gml:posList>37.52582846182154 141.01934430013557 0 37.52587872754501 141.01927573715767 0 37.525855073566674 141.01922475797622 0 37.52583649255742 141.01919171162237 0 37.525767801437375 141.01907373460048 0 37.525675076215144 141.0189132668745 0 37.52557076696575 141.01869248026318 0 37.5254758229167 141.01849151628107 0 37.525366138829725 141.01825853946664 0 37.525253523806256 141.01802163878912 0 37.52513470348787 141.0177675135643 0 37.5250064049238 141.01749663826243 0 37.52490650512896 141.01728387621804 0 37.52472104539043 141.01689017475576 0 37.52459151164429 141.01661566385403 0 37.524439667609435 141.0162923857926 0 37.5242969773845 141.0159907790535 0 37.52423156681242 141.0158516165343 0 37.52418044195229 141.01571579877037 0 37.52413745842494 141.01562402377246 0 37.52408970427599 141.01565928193935 0 37.524169760322486 141.01583140856906 0 37.52425821426749 141.01601959590266 0 37.524400900897184 141.01632119456175 0 37.524552731614534 141.01664444514924 0 37.52468227146642 141.0169189696283 0 37.52486771524777 141.01731263563124 0 37.524967642342034 141.0175254557308 0 37.525095919663286 141.0177962864817 0 37.52521475844367 141.01805045150667 0 37.525327407820036 141.01828742505108 0 37.52543706097905 141.01852033567866 0 37.525532022624084 141.0187213372038 0 37.52563964481792 141.01894913726943 0 37.5256936882234 141.01906052400682 0 37.52582846182154 141.01934430013557 0</gml:posList>
								</gml:LinearRing>
							</gml:exterior>
						</gml:Polygon>
					</gml:surfaceMember>
				</gml:MultiSurface>
			</tran:lod1MultiSurface>
			<uro:tranDataQualityAttribute>
				<uro:DataQualityAttribute>
					<uro:geometrySrcDescLod1 codeSpace="../../codelists/DataQualityAttribute_geometrySrcDesc.xml">000</uro:geometrySrcDescLod1>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">023</uro:thematicSrcDesc>
					<uro:thematicSrcDesc codeSpace="../../codelists/DataQualityAttribute_thematicSrcDesc.xml">000</uro:thematicSrcDesc>
					<uro:publicSurveyDataQualityAttribute>
						<uro:PublicSurveyDataQualityAttribute>
							<uro:srcScaleLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_srcScale.xml">1</uro:srcScaleLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">003</uro:publicSurveySrcDescLod1>
							<uro:publicSurveySrcDescLod1 codeSpace="../../codelists/PublicSurveyDataQualityAttribute_publicSurveySrcDesc.xml">023</uro:publicSurveySrcDescLod1>
						</uro:PublicSurveyDataQualityAttribute>
					</uro:publicSurveyDataQualityAttribute>
				</uro:DataQualityAttribute>
			</uro:tranDataQualityAttribute>
			<uro:roadStructureAttribute>
				<uro:RoadStructureAttribute>
					<uro:sectionType codeSpace="../../codelists/RoadStructureAttribute_sectionType.xml">1</uro:sectionType>
				</uro:RoadStructureAttribute>
			</uro:roadStructureAttribute>
		</tran:Road>
	</core:cityObjectMember>
</core:CityModel>
